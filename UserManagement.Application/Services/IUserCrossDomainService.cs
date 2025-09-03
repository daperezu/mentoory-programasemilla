namespace LinaSys.UserManagement.Application.Services;

/// <summary>
/// Service for fetching user-related data from other bounded contexts.
/// </summary>
public interface IUserCrossDomainService
{
    /// <summary>
    /// Gets the email address for a user from the Auth domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's email address, or null if not found.</returns>
    Task<string?> GetUserEmailAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the roles assigned to a user from the Auth domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of role names assigned to the user.</returns>
    Task<IReadOnlyList<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the projects assigned to a user from the Auth domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of project information.</returns>
    Task<IReadOnlyList<UserProjectInfo>> GetUserProjectsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the incubators assigned to a user from the Auth domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of incubator information.</returns>
    Task<IReadOnlyList<UserIncubatorInfo>> GetUserIncubatorsAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about a user's project assignment.
/// </summary>
public record UserProjectInfo(
    long ProjectId,
    string ProjectName,
    string Role,
    bool IsActive);

/// <summary>
/// Information about a user's incubator assignment.
/// </summary>
public record UserIncubatorInfo(
    long IncubatorId,
    string IncubatorName,
    string Role,
    bool IsActive);