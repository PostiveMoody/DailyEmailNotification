using DailyEmailNotification.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DailyEmailNotification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<EmailNotificationService>();
        services.AddScoped<IProcessEmailRequestService, ProcessEmailRequestService>();
        services.AddScoped<EmailProcessingRecoveryService>();
        services.AddScoped<OutboxEmailService>();

        return services;
    }
}