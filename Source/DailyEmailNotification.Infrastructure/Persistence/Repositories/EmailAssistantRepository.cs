using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Application.Models.AI;
using DailyEmailNotification.Application.Models.Email;
using DailyEmailNotification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyEmailNotification.Infrastructure.Persistence.Repositories;

public class EmailAssistantRepository : IEmailAssistantRepository
{
    private readonly ApplicationDbContext _context;

    public EmailAssistantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IncomingMessageResult?> TrySaveIncomingMessageAsync(
        IncomingEmail email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email.MessageId))
        {
            throw new InvalidOperationException(
                "У входящего письма отсутствует Message-ID.");
        }

        var existingMessage = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(
            x => x.ExternalMessageId == email.MessageId,
            cancellationToken);

        if (existingMessage != null)
        {
            return new IncomingMessageResult
            {
                MessageId = existingMessage.Id,
                ConversationId = existingMessage.ConversationId,
                IsDuplicate = true,
                ExistingStatus = existingMessage.Status
            };
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(
                x => x.Email == email.From,
                cancellationToken);

        if (user == null)
        {
            user = new AppUser
            {
                Email = email.From,
                IsActive = true,
            };

            _context.AppUsers.Add(user);


            user.Context = new UserContext
            {
                UserId = user.Id,
                UpdatedAt = DateTime.UtcNow,
                Summary = email.Subject + " " + email.Body,
                User = user,
            };

            _context.UserContexts.Add(user.Context);
        }
        else
        {
            var userContext = await _context.UserContexts
                .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

            if (!user.IsActive)
                return null;

            if (userContext == null)
            {
                user.Context = new UserContext
                {
                    UserId = user.Id,
                    UpdatedAt = DateTime.UtcNow,
                    Summary = email.Subject + " " + email.Body,
                    User = user,
                };

                _context.UserContexts.Add(user.Context);
            }
            else
            {
                userContext.Summary = email.Subject + " " + email.Body;
            }

        }

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(
                x => x.UserId == user.Id && x.IsActive,
                cancellationToken);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                User = user,
                Title = string.IsNullOrWhiteSpace(email.Subject)
                    ? "Новая переписка"
                    : email.Subject
            };

            _context.Conversations.Add(conversation);
        }

        var message = new Message
        {
            Conversation = conversation,
            Direction = MessageDirection.Incoming,
            Status = MessageStatus.Received,
            Subject = email.Subject,
            Body = email.Body,
            ExternalMessageId = email.MessageId
        };

        _context.Messages.Add(message);

        user.LastActivityAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Возможно, другое выполнение уже сохранило это письмо.
            _context.ChangeTracker.Clear();

            var duplicateExists = await _context.Messages
                .AnyAsync(
                    x => x.ExternalMessageId == email.MessageId,
                    cancellationToken);

            if (duplicateExists)
                return null;

            throw;
        }

        return new IncomingMessageResult
        {
            MessageId = message.Id,
            ConversationId = conversation.Id
        };
    }

    public async Task<AIRequestContext> GetAIContextAsync(
        Guid conversationId,
        Guid currentMessageId,
        CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Include(x => x.User)
            .ThenInclude(x => x.Context)
            .FirstAsync(
                x => x.Id == conversationId,
                cancellationToken);

        var history = await _context.Messages
            .Where(x =>
                x.ConversationId == conversationId &&
                x.Id != currentMessageId &&
                (x.Status == MessageStatus.Sent ||
                 x.Status == MessageStatus.Completed))
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.Direction,
                x.Body
            })
            .ToListAsync(cancellationToken);

        return new AIRequestContext
        {
            UserContext = conversation.User.Context?.Summary
                ?? string.Empty,

            ConversationHistory = history.Select(x =>
                $"{(x.Direction == MessageDirection.Incoming ? "Пользователь" : "Ассистент")}: {x.Body}"
            ).ToList()
        };
    }

    public async Task<Guid> SaveOutgoingMessageAsync(
        Guid conversationId,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            Direction = MessageDirection.Outgoing,
            Status = MessageStatus.Pending,
            Subject = subject,
            Body = body
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);

        return message.Id;
    }

    public async Task<bool> ChangeMessageStatusAsync(
        Guid messageId,
        MessageStatus expectedStatus,
        MessageStatus newStatus,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _context.Messages
            .Where(x =>
                x.Id == messageId &&
                x.Status == expectedStatus)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.ErrorMessage, errorMessage),
                cancellationToken);

        return affectedRows == 1;
    }

    /// <summary>
    /// метод поиска зависших сообщений
    /// </summary>
    /// <param name="olderThan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<Message>> GetStaleMessagesAsync(
    DateTime olderThan,
    CancellationToken cancellationToken)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(x =>
                x.CreatedAt < olderThan &&
                (x.Status == MessageStatus.Processing ||
                 x.Status == MessageStatus.Pending ||
                 x.Status == MessageStatus.Sending))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetPendingOutgoingMessagesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(x => x.Conversation)
            .ThenInclude(x => x.User)
            .Where(x =>
                x.Direction == MessageDirection.Outgoing &&
                x.Status == MessageStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}