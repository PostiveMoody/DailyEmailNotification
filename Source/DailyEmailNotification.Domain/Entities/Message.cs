namespace DailyEmailNotification.Domain.Entities
{
    /// <summary>
    /// Сообщения исходящие - входящие
    /// </summary>
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ConversationId { get; set; }

        public MessageDirection Direction { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? ExternalMessageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Conversation Conversation { get; set; } = null!;

        public MessageStatus Status { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
