using DailyEmailNotification.Domain.Entities;

namespace DailyEmailNotification.Application.Models.Email;

public class IncomingMessageResult
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public bool IsDuplicate { get; set; }

    public MessageStatus? ExistingStatus { get; set; }
}