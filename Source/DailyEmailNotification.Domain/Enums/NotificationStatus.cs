namespace DailyEmailNotification.Domain.Enums
{
    public enum NotificationStatus
    {
        Pending = 0, // Ожидает отправки
        Sent = 1, // Успешно отправлено
        Failed = 2 // 	Ошибка отправки
    }
}
