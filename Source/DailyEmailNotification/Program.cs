using DailyEmailNotification.Application;
using DailyEmailNotification.Application.Interfaces;
using DailyEmailNotification.Infrastructure.DependencyInjection;
using DailyEmailNotification.Worker.Jobs;
using Quartz;
using TimeZoneConverter;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DailyEmailNotification";
});


// Регистрация Application
builder.Services.AddApplication();

// Регистрация Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Московский часовой пояс
var moscowTimeZone = TZConvert.GetTimeZoneInfo("Europe/Moscow");

// Регистрация Quartz
builder.Services.AddQuartz(q =>
{
    // Incoming Email Job
    var incomingJobKey = new JobKey("IncomingEmailJob");

    q.AddJob<IncomingEmailJob>(opts =>
        opts.WithIdentity(incomingJobKey));

    q.AddTrigger(opts => opts
        .ForJob(incomingJobKey)
        .WithIdentity("IncomingEmailJob-trigger")
        .WithSimpleSchedule(schedule => schedule
            .WithIntervalInSeconds(30)
            .RepeatForever()));

    // Outbox Email Job
    var outboxJobKey = new JobKey("OutboxEmailJob");

    q.AddJob<OutboxEmailJob>(opts =>
        opts.WithIdentity(outboxJobKey));

    q.AddTrigger(opts => opts
        .ForJob(outboxJobKey)
        .WithIdentity("OutboxEmailJob-trigger")
        .WithSimpleSchedule(schedule => schedule
            .WithIntervalInSeconds(30)
            .RepeatForever()));

    var dailyJobKey = new JobKey("DailyEmailJob");

    q.AddJob<DailyEmailJob>(opts =>
        opts.WithIdentity(dailyJobKey));

    q.AddTrigger(opts => opts
     .ForJob(dailyJobKey)
     .WithIdentity("DailyEmailJob-trigger")
     .WithCronSchedule(
         "0 50 21 * * ?",
         x => x.InTimeZone(moscowTimeZone)));
});

// Запускаем Quartz вместе с приложением
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var host = builder.Build();

await host.RunAsync();