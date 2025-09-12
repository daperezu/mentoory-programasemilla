using System.Diagnostics.CodeAnalysis;
using LinaSys.Auth.Infrastructure.Persistence;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using LinaSys.Core.Infrastructure.Persistence;
using LinaSys.Diagnostics.Infrastructure.Persistence;
using LinaSys.KnowledgeStructure.Infrastructure.Persistence;
using LinaSys.Notification.Infrastructure.Persistence;
using LinaSys.Orchestration.Application.Diagnostics.Commands;
using LinaSys.Shared.Infrastructure.Persistence;
using LinaSys.Subscription.Infrastructure.Persistence;
using LinaSys.UserManagement.Infrastructure.Persistence;

namespace LinaSys.Web.Infrastructure.Persistence;

public class DbContextFactory(IServiceProvider serviceProvider) : IDbContextFactory
{
    private readonly Dictionary<string, Type> _mapping = new()
    {
        { "Auth", typeof(AuthDbContext) },
        { "BusinessIncubator", typeof(BusinessIncubatorDbContext) },
        { "Core", typeof(CoreDbContext) },
        { "Diagnostics", typeof(DiagnosticsDbContext) },
        { "KnowledgeStructure", typeof(KnowledgeStructureDbContext) },
        { "Notification", typeof(NotificationDbContext) },
        { "Subscription", typeof(SubscriptionDbContext) },
        { "UserManagement", typeof(UserManagementDbContext) },
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
