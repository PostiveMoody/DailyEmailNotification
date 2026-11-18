namespace DailyEmailNotification.Domain.Entities;

public class EmailRecipient
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public ICollection<EmailNotification> Notifications { get; private set; }
        = new List<EmailNotification>();

    private EmailRecipient()
    {
    }

    public static EmailRecipient Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(email));

        return new EmailRecipient
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(email));

        Email = email.Trim();
    }
}