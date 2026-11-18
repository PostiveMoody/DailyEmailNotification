using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DailyEmailNotification.Worker.Jobs;

[DisallowConcurrentExecution]
public class IncomingEmailJob : IJob
{
    private readonly IIncomingEmailReader _emailReader;
    private readonly IProcessEmailRequestService _processService;
    private readonly ILogger<IncomingEmailJob> _logger;

    public IncomingEmailJob(
        IIncomingEmailReader emailReader,
        IProcessEmailRequestService processService,
        ILogger<IncomingEmailJob> logger)
    {
        _emailReader = emailReader;
        _processService = processService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        _logger.LogInformation("Checking incoming emails...");

        var emails = await _emailReader.GetUnreadEmailsAsync(
            cancellationToken);

        foreach (var email in emails)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation(
                    "Processing email from {Sender}, subject: {Subject}",
                    email.From,
                    email.Subject);

                await _processService.ProcessAsync(
                    email,
                    cancellationToken);
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
                    "Failed to process email {MessageId}",
                    email.MessageId);
            }
        }

        _logger.LogInformation("Incoming email check completed.");
    }
}