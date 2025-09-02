using System.Reflection;
using LinaSys.Shared.Application;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.UserManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementApplication(this IServiceCollection services)
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

        return services;
    }
}
