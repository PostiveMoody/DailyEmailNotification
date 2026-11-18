using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyEmailNotification.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Direction)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(998);

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.ExternalMessageId)
            .HasMaxLength(500);

        // Защита от повторного сохранения одного входящего письма.
        builder.HasIndex(x => x.ExternalMessageId)
            .IsUnique()
            .HasFilter("[ExternalMessageId] IS NOT NULL");

        builder.HasIndex(x => x.ConversationId);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);
    }
}