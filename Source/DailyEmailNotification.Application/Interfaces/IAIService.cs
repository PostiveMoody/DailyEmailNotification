using DailyEmailNotification.Application.Models.AI;

namespace DailyEmailNotification.Application.Interfaces;

public interface IAIService
{
    Task<string> GenerateResponseAsync(
        string userMessage,
        AIRequestContext context,
        CancellationToken cancellationToken = default);
}