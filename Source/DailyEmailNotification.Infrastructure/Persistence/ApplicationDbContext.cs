using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyEmailNotification.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<EmailNotification> EmailNotifications => Set<EmailNotification>();
        public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();

        // Новые таблицы AI Assistant
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<UserContext> UserContexts => Set<UserContext>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Apply configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
