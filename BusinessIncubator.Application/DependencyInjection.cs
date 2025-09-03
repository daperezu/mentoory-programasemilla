using System.Reflection;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Services;
using LinaSys.BusinessIncubator.Application.Starter.Services;
using LinaSys.Shared.Application;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.BusinessIncubator.Application;

/// <summary>
/// Provides extension methods for setting up application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Business Incubator application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddBusinessIncubatorApplication(this IServiceCollection services)
    {
        // Register shared application services
        services.AddSharedApplication();

        // Register MediatR services from the executing assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
        });

        // Register FluentValidation validators from the executing assembly
        FluentValidation.AssemblyScanner
            .FindValidatorsInAssembly(Assembly.GetExecutingAssembly())
            .ForEach(item => services.AddScoped(item.InterfaceType, item.ValidatorType));

        // Register application services
        services.AddScoped<IDraftDataAdapter, DraftDataAdapter>();

        // Register Starter services
        services.AddScoped<ITaskGenerationService, TaskGenerationService>();
        services.AddScoped<IProgressCalculationService, ProgressCalculationService>();
        services.AddScoped<IStarterNotificationService, StarterNotificationService>();

        return services;
    }
}
