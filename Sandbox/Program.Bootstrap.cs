using System.Reflection;
using LinaSys.Auth.Application;
using LinaSys.Auth.Infrastructure;
using LinaSys.BusinessIncubator.Application;
using LinaSys.BusinessIncubator.Infrastructure;
using LinaSys.Notification.Application;
using LinaSys.Notification.Infrastructure;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.Behaviors;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Shared.Infrastructure.Behaviors;
using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.SystemFeatures.Application;
using LinaSys.SystemFeatures.Infrastructure;
using LinaSys.Web.Infrastructure.Persistence;
using LinaSys.Web.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Sandbox;

public partial class Program
{
    private static IHost _app;

    private static partial IServiceProvider Bootstrap(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        //// Go to LinaSys.Web/Program.cs and copy all the code from there to here

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

        builder.Services.AddScoped<ITimeProvider, DefaultSystemTimeProvider>();
        builder.Services.AddScoped<ICurrentUserService, CurrentHttpUserService>();
        builder.Services.AddScoped<IAuditContext>(provider =>
        {
            var timeProvider = provider.GetRequiredService<ITimeProvider>();
            var currentUserService = provider.GetRequiredService<ICurrentUserService>();

            return new AuditContext(timeProvider.UtcNow, currentUserService.UserName);
        });

        //// Notification Domain
        builder.Services.AddNotificationInfrastructure(builder.Configuration);
        builder.Services.AddNotificationApplication();

        //// Auth Domain
        builder.AddAuthInfrastructure();
        builder.Services.AddAuthApplication();

        //// BusinessIncubator Domain
        builder.AddBusinessIncubatorInfrastructure();
        builder.Services.AddBusinessIncubatorApplication();

        //// SystemFeatures Domain
        builder.AddSystemFeaturesInfrastructure();
        builder.Services.AddSystemFeaturesApplication();

        // Automatically register all classes decorated with ScopedDependencyAttribute
        RegisterScopedDependencies(builder.Services);

        _app = builder.Build();

        return _app.Services;
    }

    private static void RegisterScopedDependencies(IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var typesWithAttribute = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<ScopedDependencyAttribute>() is not null);

            foreach (var type in typesWithAttribute)
            {
                services.AddScoped(type);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScopedDependencyAttribute : Attribute
    {
    }
}
