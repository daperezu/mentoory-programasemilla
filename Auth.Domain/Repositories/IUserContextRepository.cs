using LinaSys.Auth.Domain.AggregatesModel.User;

namespace LinaSys.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for user context operations.
/// </summary>
public interface IUserContextRepository
{
    /// <summary>
    /// Gets user context preferences.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user context preferences or null if not found.</returns>
    Task<UserContextPreferences?> GetUserContextPreferencesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves user context preferences.
    /// </summary>
    /// <param name="preferences">The preferences to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveUserContextPreferencesAsync(UserContextPreferences preferences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    Task<bool> UserHasRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
}
