using System.Security.Claims;
using LinaSys.Permissions.Domain.Constants;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application.Auth;

namespace LinaSys.Web.Auth;

/// <summary>
/// Implementation of IAccessChecker using ProtectedResource for authorization.
/// This is domain-agnostic and only uses resource types and IDs.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AccessChecker"/> class.
/// </remarks>
/// <param name="protectedResourceRepository">The protected resource repository.</param>
public class AccessChecker(IProtectedResourceRepository protectedResourceRepository) : IAccessChecker
{

    /// <inheritdoc/>
    public async Task<bool> HasAccessToResourceAsync(ClaimsPrincipal principal, Guid resourceExternalId, CancellationToken cancellationToken = default)
    {
        if (!principal.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleIds = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Check if user has direct access or role-based access to the resource
        var resource = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(resourceExternalId, cancellationToken).ConfigureAwait(false);
        if (resource is null)
        {
            return false;
        }

        // Check user permissions
        if (await protectedResourceRepository.UserHasAccessAsync(userId, resource.Id, cancellationToken).ConfigureAwait(false))
        {
            return true;
        }

        // Check role permissions
        if (roleIds.Count > 0 && await protectedResourceRepository.RoleHasAccessAsync(roleIds, resource.Id, cancellationToken).ConfigureAwait(false))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> HasProjectRoleAsync(string userId, Guid projectExternalId, string role, CancellationToken cancellationToken = default)
    {
        // Projects are ProtectedResources of type Project
        // The role information would need to be stored in the ProtectedResource metadata
        // or in a separate authorization context
        // For now, check if user has access to the project resource
        var resource = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(projectExternalId, cancellationToken).ConfigureAwait(false);
        if (resource is null || resource.ResourceType != ResourceTypes.Project)
        {
            return false;
        }

        // Check if user has access to this project resource
        return await protectedResourceRepository.UserHasAccessAsync(userId, resource.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> HasProjectAccessAsync(string userId, Guid projectExternalId, CancellationToken cancellationToken = default)
    {
        var resource = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(projectExternalId, cancellationToken).ConfigureAwait(false);
        if (resource is null || resource.ResourceType != ResourceTypes.Project)
        {
            return false;
        }

        return await protectedResourceRepository.UserHasAccessAsync(userId, resource.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> HasBusinessIncubatorAccessAsync(string userId, Guid businessIncubatorExternalId, CancellationToken cancellationToken = default)
    {
        var resource = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(businessIncubatorExternalId, cancellationToken).ConfigureAwait(false);
        if (resource is null || resource.ResourceType != ResourceTypes.BusinessIncubator)
        {
            return false;
        }

        return await protectedResourceRepository.UserHasAccessAsync(userId, resource.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> GetProjectsWithRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        // Get all project resources the user has access to
        // Note: Role-specific filtering would require metadata in ProtectedResource
        // or a separate authorization context
        var resources = await protectedResourceRepository.GetResourcesByUserAndRoleAsync(
            userId,
            string.Empty, // We don't have roleId here
            ResourceTypes.Project,
            cancellationToken).ConfigureAwait(false);

        return resources.Select(r => r.ExternalId).ToList();
    }
}
