using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Application.Interfaces.Services;
using DailyEmailNotification.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DailyEmailNotification.Application.Services;

public class OutboxEmailService
{
    private readonly IEmailAssistantRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OutboxEmailService> _logger;

    public OutboxEmailService(
        IEmailAssistantRepository repository,
        IEmailSender emailSender,
        ILogger<OutboxEmailService> logger)
    {
        _repository = repository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var messages = await _repository.GetPendingOutgoingMessagesAsync(
            cancellationToken);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var claimed = await _repository.ChangeMessageStatusAsync(
                message.Id,
                MessageStatus.Pending,
                MessageStatus.Sending,
                null,
                cancellationToken);

            if (!claimed)
                continue;

            try
            {
                var recipient = message.Conversation.User.Email;

                await _emailSender.SendAsync(
                    recipient,
                    message.Subject,
                    message.Body,
                    cancellationToken);

                var sent = await _repository.ChangeMessageStatusAsync(
                    message.Id,
                    MessageStatus.Sending,
                    MessageStatus.Sent,
                    null,
                    cancellationToken);

                if (!sent)
                {
                    _logger.LogCritical(
                        "Письмо {MessageId} отправлено, но статус Sent не сохранён.",
                        message.Id);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка отправки сообщения {MessageId}. Статус Sending сохранён.",
                    message.Id);

                // Не возвращаем Sending в Pending:
                // SMTP мог принять письмо до возникновения ошибки.
            }
        }
    }
}