namespace DailyEmailNotification.Domain.Entities;

public enum MessageStatus
{
    Received = 1, // 	Письмо получено
    Processing = 2, // 	ИИ обрабатывает запрос
    Pending = 3, // 	Ответ сохранён, ожидает отправки
    Sending = 4, // 	Началась отправка
    Sent = 5, // 	Ответ отправлен
    Failed = 6, // 	Произошла ошибка
    Completed = 7 // 	Входящее письмо полностью обработано
}