using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Models.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyEmailNotification.Infrastructure.AI;

public class OllamaAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaAIService> _logger;

    public OllamaAIService(
        HttpClient httpClient,
        IOptions<OllamaSettings> options,
        ILogger<OllamaAIService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(
        string userMessage,
        AIRequestContext context,
        CancellationToken cancellationToken = default)
    {
        // Московское время (UTC+3)
        var moscowTime = DateTimeOffset.UtcNow.ToOffset(
            TimeSpan.FromHours(3));

        var currentDate = moscowTime.ToString("dd.MM.yyyy HH:mm");

        // Системный промпт
        var systemPrompt = $"""
            Ты — персональный AI Email Assistant.

            Твоя задача — анализировать входящие электронные письма
            и формировать естественные, грамотные и содержательные ответы.

            ВАЖНАЯ ИНФОРМАЦИЯ О ТЕКУЩЕЙ ДАТЕ:

            Текущая дата и время: {currentDate} (Московское время).
            Текущий год: {moscowTime.Year}.

            Правила работы с датой:
            - Всегда считай указанную дату достоверной.
            - Не определяй текущую дату по истории переписки.
            - Не используй даты из своих обучающих данных.
            - Не утверждай, что сейчас другой год.
            - Если пользователь спрашивает текущую дату или год,
              используй исключительно информацию из этого системного промпта.

            Правила формирования ответов:
            - Отвечай на языке входящего сообщения.
            - Не придумывай факты.
            - Не добавляй лишние пояснения.
            - Не повторяй историю переписки без необходимости.
            - Формируй ответ, который можно отправить по электронной почте.
            - Не упоминай, что являешься языковой моделью,
              если пользователь об этом не спрашивает.
            """;

        // Добавляем дополнительный контекст пользователя
        if (!string.IsNullOrWhiteSpace(context.UserContext))
        {
            systemPrompt += $"""

                Дополнительная информация о пользователе:
                {context.UserContext}
                """;
        }

        // Формируем сообщения для Ollama
        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = systemPrompt
            }
        };

        // История переписки
        if (context.ConversationHistory is not null)
        {
            foreach (var message in context.ConversationHistory)
            {
                if (string.IsNullOrWhiteSpace(message))
                    continue;

                messages.Add(new
                {
                    role = "user",
                    content = message
                });
            }
        }

        // Текущее сообщение
        messages.Add(new
        {
            role = "user",
            content = userMessage
        });

        // Запрос к Ollama
        var request = new
        {
            model = _settings.Model,
            messages,
            stream = false,

            // Отключаем режим размышлений
            think = false,

            options = new
            {
                num_predict = 512,
                temperature = 0.7
            }
        };

        _logger.LogInformation(
            "Отправка запроса в Ollama. Модель: {Model}. Дата: {Date}",
            _settings.Model,
            currentDate);

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Ошибка Ollama. HTTP {StatusCode}",
                (int)response.StatusCode);

            throw new HttpRequestException(
                $"Ошибка Ollama: HTTP {(int)response.StatusCode}. " +
                "Не удалось получить ответ от AI.");
        }

        using var document = JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        // Проверяем причину завершения генерации
        var doneReason = root.TryGetProperty(
            "done_reason",
            out var reasonElement)
                ? reasonElement.GetString()
                : null;

        if (doneReason == "length")
        {
            _logger.LogWarning(
                "Ollama достигла ограничения генерации токенов.");

            throw new InvalidOperationException(
                "Ollama достигла ограничения токенов до завершения генерации.");
        }

        // Получаем содержимое ответа
        if (!root.TryGetProperty("message", out var messageElement) ||
            !messageElement.TryGetProperty("content", out var contentElement))
        {
            throw new InvalidOperationException(
                "Ollama вернула ответ в неожиданном формате.");
        }

        var result = contentElement.GetString();

        if (string.IsNullOrWhiteSpace(result))
        {
            _logger.LogWarning(
                "Ollama вернула пустой ответ. DoneReason: {DoneReason}",
                doneReason ?? "неизвестно");

            throw new InvalidOperationException(
                "Ollama вернула пустой текст ответа.");
        }

        _logger.LogInformation(
            "Ответ Ollama успешно получен. Длина ответа: {Length}",
            result.Length);

        return result.Trim();
    }
}