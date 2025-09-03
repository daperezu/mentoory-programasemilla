using System.Security.Claims;

namespace LinaSys.Shared.Application.Auth;

/// <summary>
/// Provides authorization checking capabilities for commands and queries.
/// Uses existing ProtectedResource and ProjectUser for access control.
/// </summary>
public interface IAccessChecker
{
    /// <summary>
    /// Checks if the current user has permission to access a protected resource.
    /// </summary>
    /// <param name="principal">The user's claims principal.</param>
    /// <param name="resourceExternalId">The external ID of the protected resource.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> HasAccessToResourceAsync(ClaimsPrincipal principal, Guid resourceExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has a specific role in a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="role">The required role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    Task<bool> HasProjectRoleAsync(string userId, Guid projectExternalId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has any role in a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectExternalId">The project external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has any role in the project, false otherwise.</returns>
    Task<bool> HasProjectAccessAsync(string userId, Guid projectExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has access to a business incubator.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessIncubatorExternalId">The business incubator external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> HasBusinessIncubatorAccessAsync(string userId, Guid businessIncubatorExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the projects where the user has a specific role.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The required role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of project external IDs where the user has the specified role.</returns>
    Task<List<Guid>> GetProjectsWithRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
}
