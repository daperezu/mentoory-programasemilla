using LinaSys.Permissions.Domain.Aggregates.ProtectedResource;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Domain.Repositories;

public interface IProtectedResourceRepository : IRepository<ProtectedResource>
{
    /// <summary>
    /// Adds a new Protected Resource to the repository.
    /// </summary>
    /// <param name="protectedResource">The Protected Resource entity to add.</param>
    /// <returns>The added Protected Resource entity.</returns>
    ProtectedResource Add(ProtectedResource protectedResource);

    /// <summary>
    /// Updates an existing Protected Resource in the repository.
    /// </summary>
    /// <param name="protectedResource">The Protected Resource entity to update.</param>
    void Update(ProtectedResource protectedResource);

    Task<ProtectedResource?> GetProtectedResourceByExternalIdAsync(Guid externalId, CancellationToken cancellationToken);

    Task<bool> RoleHasAccessAsync(List<string> roles, long entityId, CancellationToken cancellationToken);

    Task<bool> UserHasAccessAsync(string userId, long protectedResourceId, CancellationToken cancellationToken);

    Task<ProtectedResource?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<ProtectedResource?> GetProtectedResourceWithPermissionsAsync(long id, CancellationToken cancellationToken);

    Task<(IEnumerable<ProtectedResource> Resources, int TotalCount)> ListProtectedResourcesAsync(
        int? resourceType,
        string? searchTerm,
        int skip,
        int take,
        string? orderByColumn,
        string? orderDirection,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets resources that a user has access to through direct permissions or role permissions.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role ID.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of protected resources.</returns>
    Task<List<ProtectedResource>> GetResourcesByUserAndRoleAsync(string userId, string role, int resourceType, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a user has access to a specific resource.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role ID.</param>
    /// <param name="resourceExternalId">The resource external ID.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> UserHasAccessToResourceAsync(string userId, string role, Guid resourceExternalId, int resourceType, CancellationToken cancellationToken);
}
