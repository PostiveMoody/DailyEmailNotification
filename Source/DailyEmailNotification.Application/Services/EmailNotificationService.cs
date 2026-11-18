using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Application.Interfaces.Services;
using DailyEmailNotification.Domain.Entities;

namespace DailyEmailNotification.Application.Services
{
    public class EmailNotificationService
    {
        private readonly IEmailNotificationRepository _emailNotificationRepository;
        private readonly IEmailRecipientRepository _emailRecipientRepository;
        private readonly IEmailSender _emailSender;
        public EmailNotificationService(
            IEmailNotificationRepository emailNotificationRepository,
            IEmailRecipientRepository emailRecipientRepository,
            IEmailSender emailSender)
        {
            _emailNotificationRepository = emailNotificationRepository;
            _emailRecipientRepository = emailRecipientRepository;
            _emailSender = emailSender;
        }

        public async Task SendDailyEmailNotificationsAsync(
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            // Получаем список активных получателей
            var recipients = await _emailRecipientRepository
                .GetActiveRecipientsAsync(cancellationToken);

            /// Проходим по каждому получателю и отправляем уведомление
            foreach (var recipient in recipients)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var notification = EmailNotification.Create(
                    recipient,
                    subject,
                    body);

                await _emailNotificationRepository.AddAsync(
                    notification,
                    cancellationToken);

                try
                {
                    notification.IncrementAttemptCount();

                    await _emailSender.SendAsync(
                        notification.RecipientEmail,
                        notification.Subject,
                        notification.Body,
                        cancellationToken);

                    notification.MarkAsSent();
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    notification.MarkAsFailed(exception.Message);
                }

                await _emailNotificationRepository.UpdateAsync(
                    notification,
                    cancellationToken);
            }
        }
    }
}
