namespace DailyEmailNotification.Application.Models.Email
{
    /// <summary>
    /// Входящая электронная почта
    /// </summary>
    public class IncomingEmail
    {
        /// <summary>
        /// 	Идентификатор письма из заголовка Message-ID
        /// </summary>
        public string MessageId { get; set; } = string.Empty;
        /// <summary>
        /// 	Уникальный идентификатор письма в IMAP
        /// </summary>
        public string ImapUid { get; set; } = string.Empty;
        /// <summary>
        /// 	Адрес отправителя
        /// </summary>
        public string From { get; set; } = string.Empty;
        /// <summary>
        /// Тема письма
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        /// <summary>
        /// Текст письма
        /// </summary>
        public string Body { get; set; } = string.Empty;
        /// <summary>
        /// 	Дата получения
        /// </summary>
        public DateTimeOffset ReceivedAt { get; set; }
    }
}
