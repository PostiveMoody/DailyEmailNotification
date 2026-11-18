using DailyEmailNotification.Application.Models.Email;

namespace DailyEmailNotification.Application.Services;

public interface IProcessEmailRequestService
{
    Task ProcessAsync(
        IncomingEmail email,
        CancellationToken cancellationToken = default);
}