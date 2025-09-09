namespace LinaSys.Web.Auth;

/// <summary>
/// Provides authorization checking capabilities for commands and queries.
/// Uses Auth domain services for access control.
/// </summary>
public interface IAccessChecker
{
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
    /// Checks if the current user has any role in a project (using internal ID).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project internal ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has any role in the project, false otherwise.</returns>
    Task<bool> HasProjectAccessAsync(string userId, long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has access to a business incubator (using internal ID).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessIncubatorId">The business incubator internal ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> HasBusinessIncubatorAccessAsync(string userId, long businessIncubatorId, CancellationToken cancellationToken = default);
}