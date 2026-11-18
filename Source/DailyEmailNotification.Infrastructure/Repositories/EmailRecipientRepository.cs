using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Domain.Entities;
using DailyEmailNotification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyEmailNotification.Infrastructure.Repositories
{
    public class EmailRecipientRepository : IEmailRecipientRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailRecipientRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IReadOnlyList<EmailRecipient>> GetActiveRecipientsAsync(CancellationToken cancellationToken)
        {
            return await _context.EmailRecipients
                .AsNoTracking()
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
