namespace DailyEmailNotification.Domain.Entities
{
    public class UserContext
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Summary { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
    }
}
