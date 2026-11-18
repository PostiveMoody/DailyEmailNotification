using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyEmailNotification.Infrastructure.Persistence.Configurations
{
    public class EmailNotificationConfiguration
    : IEntityTypeConfiguration<EmailNotification>
    {
        public void Configure(
            EntityTypeBuilder<EmailNotification> builder)
        {
            builder.ToTable("EmailNotifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.RecipientEmail)
                .IsRequired()
                .HasMaxLength(320);

            builder.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Body)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.AttemptCount)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.SentAtUtc);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.RecipientId,
                x.CreatedAtUtc
            });
        }
    }
}
