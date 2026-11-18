using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Application.Interfaces.Repositories;
using DailyEmailNotification.Application.Interfaces.Services;
using DailyEmailNotification.Application.Models.Email;
using DailyEmailNotification.Infrastructure.AI;
using DailyEmailNotification.Infrastructure.Email;
using DailyEmailNotification.Infrastructure.Persistence;
using DailyEmailNotification.Infrastructure.Persistence.Repositories;
using DailyEmailNotification.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace DailyEmailNotification.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register your infrastructure services here
            // For example:
            // services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();
            // services.AddScoped<IEmailRecipientRepository, EmailRecipientRepository>();
            // services.AddScoped<IEmailSender, EmailSender>();

            var connectionString = configuration
                .GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));


            services.AddScoped<IEmailRecipientRepository, EmailRecipientRepository>();

            services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();

            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

            services.AddScoped<IEmailSender, SmtpEmailSender>();

            services.Configure<OllamaSettings>(
                configuration.GetSection("Ollama"));

            services.AddHttpClient<IAIService, OllamaAIService>((serviceProvider, client) =>
            {
                var settings = serviceProvider
                    .GetRequiredService<IOptions<OllamaSettings>>()
                    .Value;

                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(5);
            });

            services.Configure<ImapSettings>(
                configuration.GetSection("Imap"));

            services.AddScoped<IIncomingEmailReader, IncomingEmailReader>();

            services.AddScoped<IEmailAssistantRepository,EmailAssistantRepository>();

            return services;
        }
    }
}
