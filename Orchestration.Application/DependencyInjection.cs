using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Orchestration.Application;

/// <summary>
/// Provides extension methods for setting up application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Orchestration application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddOrchestrationApplication(this IServiceCollection services)
    {
        // Register MediatR services from the executing assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
        });

        // Register FluentValidation validators from the executing assembly
        FluentValidation.AssemblyScanner
            .FindValidatorsInAssembly(Assembly.GetExecutingAssembly())
            .ForEach(item => services.AddScoped(item.InterfaceType, item.ValidatorType));

        return services;
    }
}
