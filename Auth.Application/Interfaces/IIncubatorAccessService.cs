namespace LinaSys.Auth.Application.Interfaces;

/// <summary>
/// Service interface for incubator access operations.
/// </summary>
public interface IIncubatorAccessService
{
    /// <summary>
    /// Gets user's accessible incubators.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of incubator information.</returns>
    Task<List<long>> GetUserActiveIncubatorsAsync(string userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a user has access to an incubator.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role identifier.</param>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user has access, false otherwise.</returns>
    Task<bool> ValidateIncubatorAccessAsync(string userId, string role, long incubatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an incubator exists.
    /// </summary>
    /// <param name="incubatorId">The incubator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> IncubatorExistsAsync(long incubatorId, CancellationToken cancellationToken = default);
}
