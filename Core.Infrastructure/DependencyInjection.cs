using LinaSys.Core.Application.Audit.Services;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Core.Domain.Aggregates.Navigation;
using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Core.Domain.Repositories;
using LinaSys.Core.Infrastructure.Persistence;
using LinaSys.Core.Infrastructure.Persistence.Repositories;
using LinaSys.Core.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<CoreDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CoreDbContext).Assembly.FullName)));

        // Add Repositories
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IWidgetRepository, WidgetRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPreferencesRepository, PreferencesRepository>();
        services.AddScoped<IUserActivityRepository, UserActivityRepository>();
        services.AddScoped<INavigationMenuRepository, NavigationMenuRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Add Services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActivityTrackingService, ActivityTrackingService>();
        services.AddScoped<IDashboardBuilderService, DashboardBuilderService>();
        services.AddScoped<IDashboardAuditService, DashboardAuditService>();
        services.AddScoped<IAuditService, AuditService>();

        // Add Memory Cache for preferences
        services.AddMemoryCache();

        return services;
    }
}
