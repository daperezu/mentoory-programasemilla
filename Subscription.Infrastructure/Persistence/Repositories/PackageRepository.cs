using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.Subscription.Infrastructure.Persistence.Repositories;

public class PackageRepository(SubscriptionDbContext dbContext)
    : AbstractRepository<Package>(dbContext), IPackageRepository
{
    /// <inheritdoc />
    public Task<Package?> GetByIdWithVersionsAndLimits(long id, CancellationToken cancellationToken)
    {
        return dbContext.Packages
            .Include(i => i.PackageVersions)
            .ThenInclude(i => i.PackageVersionLimits)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> VersionExistsAsync(long requestPackageVersionId, CancellationToken cancellationToken)
    {
        return dbContext.PackageVersions.AnyAsync(i => i.Id == requestPackageVersionId, cancellationToken);
    }

    public Task<List<PackageVersion>> GetAvailableVersionsAsync(CancellationToken cancellationToken)
    {
        return dbContext.PackageVersions
            .Include(i => i.Package)
            .ToListAsync(cancellationToken);
    }
}
