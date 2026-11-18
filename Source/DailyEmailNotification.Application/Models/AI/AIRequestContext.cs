namespace DailyEmailNotification.Application.Models.AI;

public class AIRequestContext
{
    /// <summary>
    /// Долгосрочная память пользователя;
    /// </summary>
    public string UserContext { get; set; } = string.Empty;

    /// <summary>
    /// История текущей переписки
    /// </summary>
    public List<string> ConversationHistory { get; set; } = [];
}