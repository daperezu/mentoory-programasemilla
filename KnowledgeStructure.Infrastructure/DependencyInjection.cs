using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.KnowledgeStructure.Infrastructure.Persistence;
using LinaSys.KnowledgeStructure.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinaSys.KnowledgeStructure.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddKnowledgeStructureInfrastructure(this IHostApplicationBuilder builder, string connectionName = "DefaultConnection")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        builder.Services.AddDbContext<KnowledgeStructureDbContext>(opts =>
        {
            opts.UseSqlServer(connectionString);
            opts.EnableSensitiveDataLogging();
            opts.EnableDetailedErrors();
        });

        // Aspire extension
        builder.EnrichSqlServerDbContext<KnowledgeStructureDbContext>(settings =>
        {
            settings.CommandTimeout = 30;
        });

        builder.Services.AddScoped<IKnowledgeStructureRepository, KnowledgeStructureRepository>();
        builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
        builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
        builder.Services.AddScoped<ITopicRepository, TopicRepository>();
        return builder;
    }
}
