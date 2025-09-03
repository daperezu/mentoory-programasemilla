using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Subscription.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="BusinessIncubatorPackage"/> aggregate roots.
/// </summary>
public class BusinessIncubatorPackageRepository(SubscriptionDbContext dbContext)
    : AbstractRepository<BusinessIncubatorPackage>(dbContext), IBusinessIncubatorPackageRepository
{
    /// <inheritdoc />
    public Task<BusinessIncubatorPackage?> GetByIncubatorIdAsync(long requestBusinessIncubatorId, CancellationToken cancellationToken)
    {
        return dbContext.Set<BusinessIncubatorPackage>()
            .FirstOrDefaultAsync(x => x.BusinessIncubatorId == requestBusinessIncubatorId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BusinessIncubatorPackage?> GetWithVersionAndLimitsByIncubatorIdAsync(long id, CancellationToken ct)
    {
        return dbContext.Set<BusinessIncubatorPackage>()
            .AsNoTracking()
            .Include(x => x.PackageVersion)
            .Include(x => x.PackageVersion.Package)
            .Include(x => x.PackageVersion.PackageVersionLimits)
            .Include(x => x.PackageLimitOverrides)
            .FirstOrDefaultAsync(x => x.BusinessIncubatorId == id, ct);
    }

    public Task<BusinessIncubatorPackage?> GetWithLimitOverridesByIncubatorIdAsync(long id, CancellationToken ct)
    {
        return dbContext.Set<BusinessIncubatorPackage>()
            .Include(x => x.PackageLimitOverrides)
            .FirstOrDefaultAsync(x => x.BusinessIncubatorId == id, ct);
    }

    /// <inheritdoc />
    public Task<bool> HasPackageAsync(long businessIncubatorPackageId, CancellationToken cancellationToken)
    {
        return dbContext.Set<BusinessIncubatorPackage>()
            .AnyAsync(x => x.BusinessIncubatorId == businessIncubatorPackageId, cancellationToken);
    }
}
