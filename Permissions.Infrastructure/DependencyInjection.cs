using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Permissions.Infrastructure.Persistence;
using LinaSys.Permissions.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Permissions.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPermissionsInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<PermissionsDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        // Aspire extension
        builder.EnrichSqlServerDbContext<PermissionsDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddScoped<IProtectedResourceRepository, ProtectedResourceRepository>();
        return builder;
    }
}
