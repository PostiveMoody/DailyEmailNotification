using DailyEmailNotification.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DailyEmailNotification.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailSender(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }
        
        public async Task SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.FromEmail));

            message.To.Add(
                MailboxAddress.Parse(recipient));

            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
            _settings.UserName,
            _settings.Password,
            cancellationToken);

            await client.SendAsync(message, cancellationToken);

            await client.DisconnectAsync(
                true,
                cancellationToken);
        }
    }
}
