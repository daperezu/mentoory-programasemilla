namespace LinaSys.Shared.Application.Auth;

/// <summary>
/// Provides authorization scope information following the repository pattern.
/// Uses specific methods instead of generic queries.
/// </summary>
public interface IAuthScopeProvider
{
    /// <summary>
    /// Gets the user ID from the current context.
    /// </summary>
    /// <returns>The current user ID.</returns>
    string GetCurrentUserId();

    /// <summary>
    /// Gets the user's roles from the current context.
    /// </summary>
    /// <returns>List of user roles.</returns>
    List<string> GetCurrentUserRoles();

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated, false otherwise.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Gets the current user's email.
    /// </summary>
    /// <returns>The user's email address.</returns>
    string? GetCurrentUserEmail();

    /// <summary>
    /// Gets the current user's name.
    /// </summary>
    /// <returns>The user's display name.</returns>
    string? GetCurrentUserName();

    /// <summary>
    /// Checks if the current user has a specific claim.
    /// </summary>
    /// <param name="claimType">The claim type to check.</param>
    /// <param name="claimValue">The claim value to check.</param>
    /// <returns>True if the user has the claim, false otherwise.</returns>
    bool HasClaim(string claimType, string claimValue);

    /// <summary>
    /// Gets a specific claim value from the current user.
    /// </summary>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>The claim value if found, null otherwise.</returns>
    string? GetClaimValue(string claimType);

    /// <summary>
    /// Gets all claim values of a specific type from the current user.
    /// </summary>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>List of claim values.</returns>
    List<string> GetClaimValues(string claimType);
}
