using DailyEmailNotification.Application.Models.Email;

namespace DailyEmailNotification.Application.Interfaces
{
    public interface IIncomingEmailReader
    {
        /// <summary>
        /// Получает непрочитанные письма из Gmail.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IncomingEmail>> GetUnreadEmailsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Помечает письмо прочитанным после успешной обработки.
        /// </summary>
        /// <param name="imapUid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task MarkAsReadAsync(
            string imapUid,
            CancellationToken cancellationToken = default);
    }
}
