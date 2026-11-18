using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DailyEmailNotification.Application.Services;

public class EmailProcessingRecoveryService
{
    private readonly IEmailAssistantRepository _repository;
    private readonly ILogger<EmailProcessingRecoveryService> _logger;

    public EmailProcessingRecoveryService(
        IEmailAssistantRepository repository,
        ILogger<EmailProcessingRecoveryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task RecoverAsync(
        CancellationToken cancellationToken = default)
    {
        var olderThan = DateTime.UtcNow.AddMinutes(-15);

        var staleMessages = await _repository.GetStaleMessagesAsync(
            olderThan,
            cancellationToken);

        foreach (var message in staleMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (message.Status)
            {
                case MessageStatus.Processing:
                    _logger.LogWarning(
                        "Обнаружено зависшее входящее сообщение {MessageId}.",
                        message.Id);

                    // Не переводим автоматически в Received:
                    // сначала нужно реализовать безопасный механизм
                    // повторного запуска AI-обработки.
                    break;

                case MessageStatus.Pending:
                    _logger.LogWarning(
                        "Обнаружен неотправленный ответ {MessageId}. " +
                        "Он может быть безопасно восстановлен.",
                        message.Id);

                    // Пока не отправляем автоматически.
                    // Следующим этапом добавим отдельный Outbox Worker.
                    break;

                case MessageStatus.Sending:
                    _logger.LogCritical(
                        "Обнаружено сообщение {MessageId} в статусе Sending. " +
                        "Результат отправки неизвестен. Требуется сверка.",
                        message.Id);

                    // Повторная отправка запрещена.
                    break;
            }
        }
    }
}