using DailyEmailNotification.Domain.Entities;

namespace DailyEmailNotification.Application.Interfaces.Repositories;

public interface IEmailRecipientRepository
{
    Task<IReadOnlyList<EmailRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken);
}