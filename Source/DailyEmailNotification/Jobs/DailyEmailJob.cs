using DailyEmailNotification.Application.Services;
using Quartz;

namespace DailyEmailNotification.Worker.Jobs;

[DisallowConcurrentExecution]
public class DailyEmailJob : IJob
{
    private readonly EmailNotificationService _notificationService;
    private readonly ILogger<DailyEmailJob> _logger;

    public DailyEmailJob(
        EmailNotificationService notificationService,
        ILogger<DailyEmailJob> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation(
            "Началась ежедневная отправка уведомлений.");

        try
        {
            await _notificationService.SendDailyEmailNotificationsAsync(
                "Знакомство с вашим ИИ-ассистентом",
                """
                Привет!

                Я — ваш персональный ИИ-ассистент!

                Я могу помочь вам практически в любом вопросе:
                
                - Ответить на вопросы и объяснить сложные темы.
                - Написать, отредактировать или перевести текст.
                - Помочь с программированием и поиском ошибок в коде.
                - Придумать идеи и предложить решения.
                - Помочь с обучением и саморазвитием.
                - Проанализировать информацию и структурировать данные.
                - Подготовить письма, документы и деловые сообщения.

                Вы можете просто написать мне письмо со своим вопросом или задачей.
                Я постараюсь дать полезный, понятный и подробный ответ.

                Не стесняйтесь спрашивать — я всегда готов помочь!

                С уважением,
                Ваш ИИ-ассистент
                """,
                context.CancellationToken);

            _logger.LogInformation(
                "Ежедневная отправка уведомлений завершена.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка при выполнении ежедневной отправки уведомлений.");

            throw;
        }
    }
}