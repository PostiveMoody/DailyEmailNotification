namespace DailyEmailNotification.Domain.Entities
{
    /// <summary>
    /// Пользователь приложения
    /// </summary>
    public class AppUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastActivityAt { get; set; }

        public UserContext? Context { get; set; }

        public ICollection<Conversation> Conversations { get; set; }
            = new List<Conversation>();
    }
}
