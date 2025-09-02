using LinaSys.UserManagement.Application.Queries.GetUserPreferences;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.Services;

/// <summary>
/// Implementation of email preference checking service.
/// </summary>
public class EmailPreferenceService(IMediator mediator, ILogger<EmailPreferenceService> logger) : IEmailPreferenceService
{
    public async Task<bool> CanSendEmailAsync(string userId, string preferenceKey, CancellationToken cancellationToken = default)
    {
        try
        {
            // Query user preferences from UserManagement domain
            var result = await mediator.Send(new GetUserPreferencesQuery(userId), cancellationToken);

            if (result.IsSuccess && result.Value?.TryGetValue(preferenceKey, out var value) == true)
            {
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(value, "si", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(value, "sí", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve user preferences for user {UserId}. Using default opt-in value.", userId);
        }

        // Use default behavior
        return GetDefaultOptIn(preferenceKey);
    }

    public bool GetDefaultOptIn(string preferenceKey)
    {
        // Critical emails default to opt-in
        return preferenceKey switch
        {
            PreferenceKeys.EmailPasswordReset => true,          // Always send password reset
            PreferenceKeys.EmailAccountChanges => true,         // Always send account changes
            PreferenceKeys.EmailVerificationRequired => true,   // Always send verification
            PreferenceKeys.EmailApprovals => true,              // Important: form approvals
            PreferenceKeys.EmailRejections => true,             // Important: form rejections
            PreferenceKeys.EmailSystemWelcome => true,          // Welcome emails default on
            PreferenceKeys.EmailProjectWelcome => true,         // Project welcome default on
            PreferenceKeys.EmailInvitations => true,            // Invitations default on
            PreferenceKeys.EmailFormSubmissions => true,        // Form confirmations default on
            PreferenceKeys.EmailReminders => false,             // Reminders default off
            PreferenceKeys.EmailNotifications => false,         // General notifications default off
            _ => true,
        };
    }

    // Define preference keys locally to avoid cross-domain dependency
    private static class PreferenceKeys
    {
        public const string EmailPasswordReset = "email.password.reset";
        public const string EmailAccountChanges = "email.account.changes";
        public const string EmailVerificationRequired = "email.verification.required";
        public const string EmailApprovals = "email.approvals";
        public const string EmailRejections = "email.rejections";
        public const string EmailSystemWelcome = "email.system.welcome";
        public const string EmailProjectWelcome = "email.project.welcome";
        public const string EmailInvitations = "email.invitations";
        public const string EmailFormSubmissions = "email.form.submissions";
        public const string EmailReminders = "email.reminders";
        public const string EmailNotifications = "email.notifications";
    }
}