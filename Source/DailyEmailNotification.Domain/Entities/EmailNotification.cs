using DailyEmailNotification.Domain.Enums;

namespace DailyEmailNotification.Domain.Entities;

public class EmailNotification
{
    public Guid Id { get; private set; }

    public Guid RecipientId { get; private set; }

    public string RecipientEmail { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; }

    private int _attemptCount;

    public int AttemptCount
    {
        get => _attemptCount;
        private set => _attemptCount = value;
    }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    // Навигационное свойство
    public EmailRecipient Recipient { get; private set; } = null!;

    // Конструктор для EF Core
    private EmailNotification()
    {
    }

    public static EmailNotification Create(
        EmailRecipient recipient,
        string subject,
        string body)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (!recipient.IsActive)
            throw new InvalidOperationException(
                "Cannot create notification for inactive recipient.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException(
                "Subject cannot be empty.",
                nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException(
                "Body cannot be empty.",
                nameof(body));

        return new EmailNotification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipient.Id,
            RecipientEmail = recipient.Email,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            _attemptCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            Recipient = recipient
        };
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void IncrementAttemptCount()
    {
        Interlocked.Increment(ref _attemptCount);
    }
}