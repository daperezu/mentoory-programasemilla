using System.Diagnostics.CodeAnalysis;
using LinaSys.Auth.Infrastructure.Persistence;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.KnowledgeStructure.Infrastructure.Persistence;
using LinaSys.Orchestration.Application.Diagnostics.Commands;
using LinaSys.Permissions.Infrastructure.Persistence;
using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.Subscription.Infrastructure.Persistence;
using LinaSys.SystemFeatures.Infrastructure.Persistence;

namespace LinaSys.Web.Infrastructure.Persistence;

public class DbContextFactory(IServiceProvider serviceProvider) : IDbContextFactory
{
    private readonly Dictionary<string, Type> _mapping = new()
    {
        { "Auth", typeof(AuthDbContext) },
        { "BusinessIncubator", typeof(BusinessIncubatorDbContext) },
        { "Diagnostics", typeof(DiagnosticsDbContext) },
        { "KnowledgeStructure", typeof(KnowledgeStructureDbContext) },
        { "Permissions", typeof(PermissionsDbContext) },
        { "Subscription", typeof(SubscriptionDbContext) },
        { "SystemFeatures", typeof(SystemFeaturesDbContext) },
    };

    private readonly Dictionary<Type, Type> _routeContext = new()
    {
        { typeof(UpsertDiagnosisFormFromCsvOrchestrationCommand), typeof(DiagnosticsDbContext) },
    };

    public bool TryGetDbContextForRequest<TRequest>([NotNullWhen(true)] out IDbContext? dbContext)
    {
        var requestNs = typeof(TRequest).Namespace!;
        var moduleName = requestNs.Split('.')[1];

        var dbContextType = _mapping.FirstOrDefault(x => x.Key == moduleName).Value;

        if (dbContextType is null)
        {
            if (!_routeContext.TryGetValue(typeof(TRequest), out dbContextType))
            {
                dbContext = null;
                return false;
            }
        }

        dbContext = (IDbContext)serviceProvider.GetRequiredService(dbContextType);
        return true;
    }
}
