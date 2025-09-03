using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

/// <summary>
/// Repository interface for managing <see cref="Package"/> aggregate roots.
/// Provides methods for adding, updating, and retrieving packages and their versions.
/// </summary>
public interface IPackageRepository : IRepository<Package>
{
    /// <summary>
    /// Adds a new package to the repository.
    /// </summary>
    /// <param name="package">The package to add.</param>
    /// <returns>The added package.</returns>
    Package Add(Package package);

    /// <summary>
    /// Finds a package by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the package.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the package if found; otherwise, <c>null</c>.
    /// </returns>
    ValueTask<Package?> FindByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a package by its identifier, including its versions and limits.
    /// </summary>
    /// <param name="id">The identifier of the package.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the package if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Package?> GetByIdWithVersionsAndLimits(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing package in the repository.
    /// </summary>
    /// <param name="package">The package to update.</param>
    void Update(Package package);

    /// <summary>
    /// Checks if a specific package version exists in the repository.
    /// </summary>
    /// <param name="requestPackageVersionId">The identifier of the package version to check.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <c>true</c> if the package version exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> VersionExistsAsync(long requestPackageVersionId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all available versions of packages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of available <see cref="PackageVersion"/> objects.
    /// </returns>
    Task<List<PackageVersion>> GetAvailableVersionsAsync(CancellationToken cancellationToken);
}
