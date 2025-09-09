using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Notification.Infrastructure.Persistence;
using LinaSys.Notification.Infrastructure.Persistence.Repositories;
using LinaSys.Notification.Infrastructure.Services;
using LinaSys.Notification.Infrastructure.Workers;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)));

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork>(provider => provider.GetService<NotificationDbContext>()!);

        // Repositories
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // Email queue and worker services
        services.AddSingleton<EmailQueueService>();
        services.AddSingleton<IEmailQueueService>(sp => sp.GetRequiredService<EmailQueueService>()); // Ensure shared instance
        services.AddHostedService<EmailSenderWorker>(); // Background worker for processing queued emails

        // Email transports
        services.AddEmailTransports(configuration, false);

        // Notification domain services
        services.AddScoped<IEmailPreferenceService, EmailPreferenceService>();

        // Email template service - always use database implementation
        services.AddScoped<IEmailTemplateService, DatabaseEmailTemplateService>();

        // Note: MediatR handlers are registered in Notification.Application.DependencyInjection
        // Do not register them here to avoid duplicate registrations
        return services;
    }
}
