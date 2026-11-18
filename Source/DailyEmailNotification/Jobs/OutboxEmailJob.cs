using DailyEmailNotification.Application.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DailyEmailNotification.Worker.Jobs;

[DisallowConcurrentExecution]
public class OutboxEmailJob : IJob
{
    private readonly OutboxEmailService _outboxService;
    private readonly ILogger<OutboxEmailJob> _logger;

    public OutboxEmailJob(
        OutboxEmailService outboxService,
        ILogger<OutboxEmailJob> logger)
    {
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        try
        {
            _logger.LogInformation(
                "Начинаем обработку Outbox.");

            await _outboxService.ProcessPendingAsync(
                cancellationToken);

            _logger.LogInformation(
                "Обработка Outbox завершена.");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при обработке Outbox.");

            throw;
        }
    }
}