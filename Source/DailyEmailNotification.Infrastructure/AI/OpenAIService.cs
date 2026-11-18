using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Models.AI;
using OpenAI.Chat;

namespace DailyEmailNotification.Infrastructure.AI
{
    public class OpenAIService : IAIService
    {
        private readonly ChatClient _chatClient;

        public OpenAIService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> GenerateResponseAsync(
            string userMessage,
            string? userContext = null,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    """
                    Ты — AI Email Assistant, персональный интеллектуальный помощник.

                    Твои задачи:
                    - Отвечать на вопросы пользователя.
                    - Помогать в обучении и работе.
                    - Давать практические рекомендации.
                    - Помогать планировать задачи и время.
                    - Учитывать историю общения и сохранённую память пользователя.

                    Отвечай понятно, структурированно и доброжелательно.
                    Если информации недостаточно, задавай уточняющие вопросы.
                    """)
            };

            if (!string.IsNullOrWhiteSpace(userContext))
            {
                messages.Add(
                    new SystemChatMessage(
                        $"Контекст пользователя:\n{userContext}"));
            }

            messages.Add(new UserChatMessage(userMessage));

            var completionResult = await _chatClient.CompleteChatAsync(
                messages,
                cancellationToken: cancellationToken);

            var completion = completionResult.Value;

            var response = string.Join(
               "\n",
               completion.Content.Select(x => x.Text));

            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidOperationException(
                    "OpenAI returned an empty response.");
            }

            return response;
        }

        public Task<string> GenerateResponseAsync(string userMessage, AIRequestContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
