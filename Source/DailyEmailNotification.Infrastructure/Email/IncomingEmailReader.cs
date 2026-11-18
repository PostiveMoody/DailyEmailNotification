using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Models.Email;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;

namespace DailyEmailNotification.Infrastructure.Email;

public class IncomingEmailReader : IIncomingEmailReader
{
    private readonly ImapSettings _settings;

    public IncomingEmailReader(IOptions<ImapSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<IncomingEmail>> GetUnreadEmailsAsync(
        CancellationToken cancellationToken)
    {
        var emails = new List<IncomingEmail>();

        using var client = new ImapClient();

        client.Timeout = 30000;

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.SslOnConnect,
            cancellationToken);

        await client.AuthenticateAsync(
            _settings.UserName,
            _settings.Password,
            cancellationToken);

        var inbox = client.Inbox;

        await inbox.OpenAsync(
            FolderAccess.ReadWrite,
            cancellationToken);

        // Получаем UID непрочитанных писем.
        var uids = await inbox.SearchAsync(
            SearchQuery.NotSeen,
            cancellationToken);

        // Сначала обрабатываем самые новые письма.
        var orderedUids = uids
            .OrderByDescending(x => x.Id)
            .Take(_settings.MaxEmailsPerCheck);

        foreach (var uid in orderedUids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Сначала получаем заголовки, чтобы проверить отправителя.
            var summaries = await inbox.FetchAsync(
                new[] { uid },
                MessageSummaryItems.Envelope,
                cancellationToken);

            var summary = summaries.FirstOrDefault();

            if (summary?.Envelope == null)
                continue;

            var from = summary.Envelope.From
                .Mailboxes
                .FirstOrDefault()?.Address;

            if (string.IsNullOrWhiteSpace(from))
                continue;

            // Проверяем разрешённых отправителей.
            var isAllowed = _settings.AllowedSenders.Any(
                x => string.Equals(
                    x,
                    from,
                    StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
                continue;

            // Получаем полное письмо.
            var message = await inbox.GetMessageAsync(
                uid,
                cancellationToken);

            var body = ExtractText(message);

            // Ограничиваем размер текста.
            if (body.Length > _settings.MaxBodyLength)
            {
                body = body[.._settings.MaxBodyLength];
            }

            emails.Add(new IncomingEmail
            {
                MessageId = message.MessageId ?? string.Empty,
                ImapUid = uid.Id.ToString(),
                From = from,
                Subject = message.Subject ?? string.Empty,
                Body = body,
                ReceivedAt = message.Date.UtcDateTime
            });
        }

        await client.DisconnectAsync(
            true,
            cancellationToken);

        return emails;
    }

    public async Task MarkAsReadAsync(
        string imapUid,
        CancellationToken cancellationToken)
    {
        if (!uint.TryParse(imapUid, out var uidValue))
        {
            throw new ArgumentException(
                "Некорректный IMAP UID.",
                nameof(imapUid));
        }

        using var client = new ImapClient();

        client.Timeout = 30000;

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.SslOnConnect,
            cancellationToken);

        await client.AuthenticateAsync(
            _settings.UserName,
            _settings.Password,
            cancellationToken);

        var inbox = client.Inbox;

        await inbox.OpenAsync(
            FolderAccess.ReadWrite,
            cancellationToken);

        var uid = new UniqueId(uidValue);

        await inbox.AddFlagsAsync(
            uid,
            MessageFlags.Seen,
            true,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }

    private static string ExtractText(MimeMessage message)
    {
        // Сначала пытаемся получить обычный текст.
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            return RemoveQuotedText(message.TextBody);
        }

        // Если обычного текста нет, извлекаем его из HTML.
        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            return ExtractTextFromHtml(message.HtmlBody);
        }

        return string.Empty;
    }

    private static string ExtractTextFromHtml(string html)
    {
        var document = new HtmlDocument();

        document.LoadHtml(html);

        // Удаляем цитируемую переписку Gmail.
        var quotedNodes = document.DocumentNode.SelectNodes(
            "//blockquote | " +
            "//*[contains(concat(' ', normalize-space(@class), ' '), ' gmail_quote ')] | " +
            "//*[contains(concat(' ', normalize-space(@class), ' '), ' gmail_quote_container ')]");

        if (quotedNodes != null)
        {
            foreach (var node in quotedNodes.ToList())
            {
                node.Remove();
            }
        }

        // Удаляем технические HTML-элементы.
        var unwantedNodes = document.DocumentNode.SelectNodes(
            "//script | //style | //head");

        if (unwantedNodes != null)
        {
            foreach (var node in unwantedNodes.ToList())
            {
                node.Remove();
            }
        }

        var text = HtmlEntity.DeEntitize(
            document.DocumentNode.InnerText);

        text = Regex.Replace(text, @"[ \t]+", " ");
        text = Regex.Replace(text, @"\r\n|\r", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        return RemoveQuotedText(text);
    }

    private static string RemoveQuotedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var lines = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        var result = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Стандартное цитирование
            if (trimmedLine.StartsWith(">"))
                break;

            // Англоязычный формат Gmail
            if (Regex.IsMatch(
                    trimmedLine,
                    @"^On .+wrote:\s*$",
                    RegexOptions.IgnoreCase))
                break;

            // Русскоязычный формат Gmail:
            // ср, 18 нояб. 2026 г. в 21:30, ...
            if (Regex.IsMatch(
                    trimmedLine,
                    @"^(пн|вт|ср|чт|пт|сб|вс),?\s+\d{1,2}\s+\p{L}+\.?\s+\d{4}",
                    RegexOptions.IgnoreCase))
                break;

            // Стандартные заголовки пересланных писем
            if (trimmedLine.StartsWith("-----Original Message-----",
                    StringComparison.OrdinalIgnoreCase))
                break;

            result.Add(line);
        }

        return string.Join("\n", result).Trim();
    }

    private static bool IsReplyHeader(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        // Англоязычный Gmail:
        // On ... wrote:
        if (Regex.IsMatch(
            line,
            @"^On .{3,}wrote:\s*$",
            RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Русскоязычный Gmail:
        // ср, 18 нояб. 2026 г. в 20:58, Имя <email>:
        var russianReplyHeader = Regex.IsMatch(
            line,
            @"^(пн|вт|ср|чт|пт|сб|вс),?\s+.{0,200}\d{1,2}:\d{2}.{0,200}:$",
            RegexOptions.IgnoreCase);

        return russianReplyHeader;
    }
}