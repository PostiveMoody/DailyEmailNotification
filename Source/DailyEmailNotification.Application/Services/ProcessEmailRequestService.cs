using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Application.Interfaces.Services;
using DailyEmailNotification.Application.Models.Email;
using DailyEmailNotification.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DailyEmailNotification.Application.Services;

public class ProcessEmailRequestService : IProcessEmailRequestService
{
    private readonly IEmailAssistantRepository _repository;
    private readonly IAIService _aiService;
    private readonly IIncomingEmailReader _emailReader;
    private readonly ILogger<ProcessEmailRequestService> _logger;

    public ProcessEmailRequestService(
        IEmailAssistantRepository repository,
        IAIService aiService,
        IIncomingEmailReader emailReader,
        ILogger<ProcessEmailRequestService> logger)
    {
        _repository = repository;
        _aiService = aiService;
        _emailReader = emailReader;
        _logger = logger;
    }

    public async Task ProcessAsync(
        IncomingEmail email,
        CancellationToken cancellationToken = default)
    {
        var incoming = await _repository.TrySaveIncomingMessageAsync(
            email,
            cancellationToken);

        if (incoming == null)
        {
            _logger.LogInformation(
                "Письмо от {Sender} пропущено: отправитель неактивен.",
                email.From);

            return;
        }

        if (incoming.IsDuplicate)
        {
            if (incoming.ExistingStatus == MessageStatus.Completed)
            {
                await _emailReader.MarkAsReadAsync(
                    email.ImapUid,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Письмо {MessageId} уже существует. Статус: {Status}.",
                email.MessageId,
                incoming.ExistingStatus);

            return;
        }

        var incomingId = incoming.MessageId;

        try
        {
            var claimed = await _repository.ChangeMessageStatusAsync(
                incomingId,
                MessageStatus.Received,
                MessageStatus.Processing,
                null,
                cancellationToken);

            if (!claimed)
            {
                _logger.LogWarning(
                    "Не удалось захватить сообщение {MessageId}.",
                    email.MessageId);

                return;
            }

            var context = await _repository.GetAIContextAsync(
                incoming.ConversationId,
                incomingId,
                cancellationToken);

            _logger.LogInformation(
                "Передаём письмо от {Sender} локальной модели.",
                email.From);

            var aiResponse = await _aiService.GenerateResponseAsync(
                email.Body,
                context,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                throw new InvalidOperationException(
                    "ИИ вернул пустой ответ.");
            }

            var subject = string.IsNullOrWhiteSpace(email.Subject)
                ? "Ответ AI-ассистента"
                : $"Re: {email.Subject}";

            // Сохраняем ответ в Outbox со статусом Pending.
            await _repository.SaveOutgoingMessageAsync(
                incoming.ConversationId,
                subject,
                aiResponse,
                cancellationToken);

            // Входящее письмо обработано, ответ надёжно сохранён.
            var completed = await _repository.ChangeMessageStatusAsync(
                incomingId,
                MessageStatus.Processing,
                MessageStatus.Completed,
                null,
                cancellationToken);

            if (!completed)
            {
                throw new InvalidOperationException(
                    "Не удалось завершить обработку входящего письма.");
            }

            await _emailReader.MarkAsReadAsync(
                email.ImapUid,
                cancellationToken);

            _logger.LogInformation(
                "Ответ для {Recipient} сохранён в Outbox.",
                email.From);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка обработки письма {MessageId}.",
                email.MessageId);

            await _repository.ChangeMessageStatusAsync(
                incomingId,
                MessageStatus.Processing,
                MessageStatus.Failed,
                ex.Message,
                cancellationToken);

            throw;
        }
    }
}