namespace DailyEmailNotification.Application.Models.Email
{
    public class ImapSettings
    {
        /// <summary>
        /// Хост 
        /// </summary>
        public string Host { get; set; } = "imap.gmail.com";

        /// <summary>
        /// Порт
        /// </summary>
        public int Port { get; set; } = 993;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Разрешенные отправители
        /// </summary>
        public List<string> AllowedSenders { get; set; } = [];
        /// <summary>
        /// Максимальная длинна письма
        /// </summary>
        public int MaxBodyLength { get; set; } = 10000;
        /// <summary>
        /// Макс кол-во писем за раз
        /// </summary>
        public int MaxEmailsPerCheck { get; set; } = 5;
    }
}
