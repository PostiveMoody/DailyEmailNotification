using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyEmailNotification.Infrastructure.Persistence.Configurations;

public class UserContextConfiguration : IEntityTypeConfiguration<UserContext>
{
    public void Configure(EntityTypeBuilder<UserContext> builder)
    {
        builder.ToTable("UserContexts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Summary)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}