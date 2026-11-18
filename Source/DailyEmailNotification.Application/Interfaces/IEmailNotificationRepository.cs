using DailyEmailNotification.Domain.Entities;

namespace DailyEmailNotification.Application.Interfaces.Repositories;

public interface IEmailNotificationRepository
{
    Task AddAsync(
        EmailNotification notification,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        EmailNotification notification,
        CancellationToken cancellationToken);
}