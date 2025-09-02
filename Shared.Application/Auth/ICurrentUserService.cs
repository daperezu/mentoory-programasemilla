namespace LinaSys.Shared.Application.Auth;

/// <summary>
/// Interface for accessing information about the current user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the user ID of the current user (GUID as string).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the username (PIN) of the current user.
    /// </summary>
    string? UserName { get; }
}
