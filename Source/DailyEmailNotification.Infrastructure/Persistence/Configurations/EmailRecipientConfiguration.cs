using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyEmailNotification.Infrastructure.Persistence.Configurations
{
    public class EmailRecipientConfiguration : IEntityTypeConfiguration<EmailRecipient>
    {
        public void Configure(EntityTypeBuilder<EmailRecipient> builder)
        {
            builder.ToTable("EmailRecipients");

            builder.HasKey(e => e.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Email)
           .IsRequired()
           .HasMaxLength(320);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasMany(x => x.Notifications)
                .WithOne(x => x.Recipient)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
