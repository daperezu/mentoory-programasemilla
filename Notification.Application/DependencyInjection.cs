using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        // Note: IEmailTemplateService is registered in Infrastructure layer using DatabaseEmailTemplateService
        return services;
    }
}
