using LinaSys.Auth.Application.Interfaces;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Auth.Infrastructure.Persistence;
using LinaSys.Auth.Infrastructure.Persistence.Repositories;
using LinaSys.Auth.Infrastructure.Repositories;
using LinaSys.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddAuthInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<AuthDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);

            //// opts.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            //// opts.EnableThreadSafetyChecks(false);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        //// Aspire extension
        builder.EnrichSqlServerDbContext<AuthDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddDefaultIdentity<User>(
            opts =>
            {
                opts.SignIn.RequireConfirmedEmail = true;
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "0123456789";
                })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>()
            ;

        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IUserContextRepository, UserContextRepository>();

        // Register access services
        builder.Services.AddScoped<IIncubatorAccessService, IncubatorAccessService>();
        builder.Services.AddScoped<IProjectAccessService, ProjectAccessService>();

        return builder;
    }
}
