using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinaSys.Core.Domain.Aggregates.Navigation;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Core.Infrastructure.Persistence.Repositories;

public class NavigationMenuRepository(CoreDbContext context) : AbstractRepository<NavigationMenuItem>(context), INavigationMenuRepository
{
    private readonly CoreDbContext dbContext = context;
    public async Task<IEnumerable<NavigationMenuItem>> GetActiveMenuTreeAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NavigationMenuItems
            .Include(x => x.Parent)
            .Include(x => x.Children)
            .Where(x => x.IsActive)
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NavigationMenuItem>> GetMenuItemsByRolesAsync(
        string[] roles,
        CancellationToken cancellationToken = default)
    {
        if (roles == null || !roles.Any())
        {
            return await GetActiveMenuTreeAsync(cancellationToken);
        }

        var query = dbContext.NavigationMenuItems
            .Include(x => x.Parent)
            .Include(x => x.Children)
            .Where(x => x.IsActive);

        // This is simplified - in production would need better SQL
        var items = await query.ToListAsync(cancellationToken);
        return items.Where(x => x.IsAllowedForRoles(roles));
    }

    public async Task<NavigationMenuItem?> GetMenuItemWithChildrenAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NavigationMenuItems
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<NavigationMenuItem?> GetMenuItemByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NavigationMenuItems
            .FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<NavigationMenuItem>> GetRootMenuItemsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.NavigationMenuItems
            .Include(x => x.Children)
            .Where(x => x.ParentId == null && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        string code,
        long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.NavigationMenuItems
            .Where(x => x.Code == code.ToUpperInvariant());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
