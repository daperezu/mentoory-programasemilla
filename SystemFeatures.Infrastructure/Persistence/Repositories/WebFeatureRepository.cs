using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using LinaSys.SystemFeatures.Domain.AggregatesModel.WebFeatureAggregate;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.SystemFeatures.Infrastructure.Persistence.Repositories;

public class WebFeatureRepository(SystemFeaturesDbContext dbContext)
    : AbstractRepository<WebFeature>(dbContext), IWebFeatureRepository
{
    public Task<WebFeature?> GetByAreaControllerAndActionAsync(string area, string controller, string action, CancellationToken cancellationToken)
    {
        return dbContext.WebFeatures
            .AsNoTracking()
            .Where(f => f.Area == area && f.Controller == controller && f.Action == action)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebFeature>> GetActiveMenuItemsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.WebFeatures
            .AsNoTracking()
            .Where(f => f.IsMenu)
            .OrderBy(f => f.MenuOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebFeature>> GetMenuHierarchyAsync(CancellationToken cancellationToken)
    {
        return await dbContext.WebFeatures
            .AsNoTracking()
            .Include(f => f.InverseParent)
            .Where(f => f.IsMenu)
            .OrderBy(f => f.MenuOrder)
            .ToListAsync(cancellationToken);
    }

    public Task<WebFeature?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return dbContext.WebFeatures
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WebFeature>> GetByParentIdAsync(long? parentId, CancellationToken cancellationToken)
    {
        return await dbContext.WebFeatures
            .AsNoTracking()
            .Where(f => f.ParentId == parentId && f.IsMenu)
            .OrderBy(f => f.MenuOrder)
            .ToListAsync(cancellationToken);
    }
}
