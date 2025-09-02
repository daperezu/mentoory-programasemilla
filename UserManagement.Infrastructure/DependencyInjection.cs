using LinaSys.UserManagement.Application.Services;
using LinaSys.UserManagement.Domain.Repositories;
using LinaSys.UserManagement.Infrastructure.Persistence;
using LinaSys.UserManagement.Infrastructure.Persistence.Repositories;
using LinaSys.UserManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddUserManagementInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<UserManagementDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        // Aspire extension
        builder.EnrichSqlServerDbContext<UserManagementDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        builder.Services.AddSingleton<IAvatarStorageService, AzureBlobAvatarStorageService>();

        return builder;
    }
}
