using LinaSys.Shared.Application.IntegrationEvents;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Shared.Application;

/// <summary>
/// Extension methods for registering shared application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers shared application services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        // Register integration event service
        services.AddScoped<IIntegrationEventService, MediatRIntegrationEventService>();

        // Register shared services
        services.AddSingleton<IPasswordGeneratorService, PasswordGeneratorService>();

        return services;
    }
}
