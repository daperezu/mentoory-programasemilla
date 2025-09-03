using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;

/// <summary>
/// Repository interface for managing <see cref="BusinessIncubatorPackage"/> aggregate roots.
/// </summary>
public interface IBusinessIncubatorPackageRepository : IRepository<BusinessIncubatorPackage>
{
    /// <summary>
    /// Adds a new business incubator package to the repository.
    /// </summary>
    /// <param name="businessIncubatorPackage">The business incubator package to add.</param>
    /// <returns>The added business incubator package.</returns>
    BusinessIncubatorPackage Add(BusinessIncubatorPackage businessIncubatorPackage);

    /// <summary>
    /// Finds a business incubator package by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the business incubator package.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the business incubator package if found; otherwise, null.</returns>
    ValueTask<BusinessIncubatorPackage?> FindByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a business incubator package by its incubator identifier.
    /// </summary>
    /// <param name="requestBusinessIncubatorId">The identifier of the business incubator.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the business incubator package if found; otherwise, null.</returns>
    Task<BusinessIncubatorPackage?> GetByIncubatorIdAsync(long requestBusinessIncubatorId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a business incubator package by its incubator identifier, including its version and limits.
    /// </summary>
    /// <param name="id">The identifier of the business incubator.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the business incubator package if found; otherwise, null.</returns>
    Task<BusinessIncubatorPackage?> GetWithVersionAndLimitsByIncubatorIdAsync(long id, CancellationToken ct);

    /// <summary>
    /// Gets a business incubator package by its incubator identifier, including its limit overrides.
    /// </summary>
    /// <param name="id">The identifier of the business incubator.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the business incubator package if found; otherwise, null.</returns>
    Task<BusinessIncubatorPackage?> GetWithLimitOverridesByIncubatorIdAsync(long id, CancellationToken ct);

    /// <summary>
    /// Checks if a business incubator package exists by its identifier.
    /// </summary>
    /// <param name="businessIncubatorPackageId">The identifier of the business incubator package.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the package exists; otherwise, false.</returns>
    Task<bool> HasPackageAsync(long businessIncubatorPackageId, CancellationToken cancellationToken);
}
