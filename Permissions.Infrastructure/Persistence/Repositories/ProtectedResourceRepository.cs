using LinaSys.Permissions.Domain.Aggregates.ProtectedResource;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Permissions.Infrastructure.Persistence.Repositories;

public class ProtectedResourceRepository(PermissionsDbContext dbContext)
    : AbstractRepository<ProtectedResource>(dbContext), IProtectedResourceRepository
{
    public new ProtectedResource Add(ProtectedResource protectedResource)
    {
        return base.Add(protectedResource);
    }

    public new void Update(ProtectedResource protectedResource)
    {
        base.Update(protectedResource);
    }

    public async Task<ProtectedResource?> GetProtectedResourceByExternalIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        return await dbContext.ProtectedResources
            .Where(e => e.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> RoleHasAccessAsync(List<string> roles, long entityId, CancellationToken cancellationToken)
    {
        return await dbContext.RoleProtectedResourcePermissions
            .AsNoTracking()
            .AnyAsync(rp => roles.Contains(rp.Role) && rp.ProtectedResourceId == entityId, cancellationToken);
    }

    public async Task<bool> UserHasAccessAsync(string userId, long protectedResourceId, CancellationToken cancellationToken)
    {
        return await dbContext.UserProtectedResourcePermissions
            .AsNoTracking()
            .AnyAsync(ep => ep.UserId == userId && ep.ProtectedResourceId == protectedResourceId, cancellationToken);
    }

    public async Task<ProtectedResource?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.ProtectedResources
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProtectedResource?> GetProtectedResourceWithPermissionsAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.ProtectedResources
            .Include(p => p.UserProtectedResourcePermissions)
            .Include(p => p.RoleProtectedResourcePermissions)
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ProtectedResource> Resources, int TotalCount)> ListProtectedResourcesAsync(
        int? resourceType,
        string? searchTerm,
        int skip,
        int take,
        string? orderByColumn,
        string? orderDirection,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ProtectedResources
            .Include(p => p.UserProtectedResourcePermissions)
            .Include(p => p.RoleProtectedResourcePermissions)
            .AsQueryable();

        if (resourceType.HasValue)
        {
            query = query.Where(p => p.ResourceType == resourceType.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering
        query = ApplyOrdering(query, orderByColumn, orderDirection);

        var resources = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (resources, totalCount);
    }

    public async Task<List<ProtectedResource>> GetResourcesByUserAndRoleAsync(string userId, string role, int resourceType, CancellationToken cancellationToken)
    {
        return await dbContext.ProtectedResources
            .Include(p => p.UserProtectedResourcePermissions)
            .Include(p => p.RoleProtectedResourcePermissions)
            .Where(p => p.ResourceType == resourceType)
            .Where(p =>
                p.UserProtectedResourcePermissions.Any(upp => upp.UserId == userId) ||
                p.RoleProtectedResourcePermissions.Any(rpp => rpp.Role == role))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasAccessToResourceAsync(string userId, string role, Guid resourceExternalId, int resourceType, CancellationToken cancellationToken)
    {
        return await dbContext.ProtectedResources
            .Where(p => p.ResourceType == resourceType && p.ExternalId == resourceExternalId)
            .AnyAsync(p =>
                p.UserProtectedResourcePermissions.Any(upp => upp.UserId == userId) ||
                p.RoleProtectedResourcePermissions.Any(rpp => rpp.Role == role),
                cancellationToken);
    }

    private static IQueryable<ProtectedResource> ApplyOrdering(IQueryable<ProtectedResource> query, string? orderByColumn, string? orderDirection)
    {
        var isDescending = string.Equals(orderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return orderByColumn?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "resourcetype" => isDescending ? query.OrderByDescending(p => p.ResourceType) : query.OrderBy(p => p.ResourceType),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "createdby" => isDescending ? query.OrderByDescending(p => p.CreatedBy) : query.OrderBy(p => p.CreatedBy),
            _ => query.OrderBy(p => p.Name), // Default ordering
        };
    }
}
