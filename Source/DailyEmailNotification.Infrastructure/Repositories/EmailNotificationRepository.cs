using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Domain.Entities;
using DailyEmailNotification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyEmailNotification.Infrastructure.Repositories
{
    public class EmailNotificationRepository : IEmailNotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public EmailNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(EmailNotification notification, CancellationToken cancellationToken)
        {

            // Получатель уже существует в базе.
            // Говорим EF Core, что его нужно только отслеживать,
            // а не добавлять повторно.
            var recipientEntry = _context.Entry(notification.Recipient);

            if (recipientEntry.State == EntityState.Detached)
            {
                _context.EmailRecipients.Attach(notification.Recipient);
            }

            await _context.EmailNotifications.AddAsync(notification, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(EmailNotification notification, CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
