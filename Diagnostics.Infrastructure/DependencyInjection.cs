using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Diagnostics.Domain.Services;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.Diagnostics.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.Diagnostics.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddDiagnosticsInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<DiagnosticsDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        // Aspire extension
        builder.EnrichSqlServerDbContext<DiagnosticsDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddScoped<IBlockRepository, BlockRepository>();
        builder.Services.AddScoped<IFormRepository, FormRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<IUserProjectDiagnosisRepository, UserProjectDiagnosisRepository>();

        // Register memory cache
        builder.Services.AddMemoryCache();

        return builder;
    }
}
