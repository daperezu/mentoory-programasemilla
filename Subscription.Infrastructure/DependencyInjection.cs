using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using LinaSys.Subscription.Infrastructure.Persistence;
using LinaSys.Subscription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Subscription.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddSubscriptionInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<SubscriptionDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);

            //// opts.EnableThreadSafetyChecks(false);
            //// opts.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        //// Aspire extension
        builder.EnrichSqlServerDbContext<SubscriptionDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddScoped<IPackageRepository, PackageRepository>();
        builder.Services.AddScoped<IBusinessIncubatorPackageRepository, BusinessIncubatorPackageRepository>();
        return builder;
    }
}
