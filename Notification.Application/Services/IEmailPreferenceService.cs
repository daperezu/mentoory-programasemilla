namespace LinaSys.Notification.Application.Services;

/// <summary>
/// Service for checking email preferences in the notification layer.
/// </summary>
public interface IEmailPreferenceService
{
    /// <summary>
    /// Checks if an email should be sent based on user preferences.
    /// </summary>
    /// <param name="userId">The user ID to check preferences for.</param>
    /// <param name="preferenceKey">The preference key to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email should be sent, false otherwise.</returns>
    Task<bool> CanSendEmailAsync(string userId, string preferenceKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default opt-in value for a preference key.
    /// </summary>
    /// <param name="preferenceKey">The preference key.</param>
    /// <returns>True if the preference defaults to opt-in, false otherwise.</returns>
    bool GetDefaultOptIn(string preferenceKey);
}