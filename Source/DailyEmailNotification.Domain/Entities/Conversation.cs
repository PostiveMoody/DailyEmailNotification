namespace DailyEmailNotification.Domain.Entities
{
    /// <summary>
    /// Диалог - чат
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
