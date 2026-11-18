using DailyEmailNotification.Application.Models.AI;
using DailyEmailNotification.Application.Models.Email;
using DailyEmailNotification.Domain.Entities;

namespace DailyEmailNotification.Application.Interfaces.Repositories;

public interface IEmailAssistantRepository
{
    Task<IncomingMessageResult?> TrySaveIncomingMessageAsync(
        IncomingEmail email,
        CancellationToken cancellationToken);

    Task<AIRequestContext> GetAIContextAsync(
        Guid conversationId,
        Guid currentMessageId,
        CancellationToken cancellationToken);

    Task<Guid> SaveOutgoingMessageAsync(
        Guid conversationId,
        string subject,
        string body,
        CancellationToken cancellationToken);

    Task<bool> ChangeMessageStatusAsync(
        Guid messageId,
        MessageStatus expectedStatus,
        MessageStatus newStatus,
        string? errorMessage,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Message>> GetStaleMessagesAsync(
        DateTime olderThan,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Message>> GetPendingOutgoingMessagesAsync(
        CancellationToken cancellationToken);
}