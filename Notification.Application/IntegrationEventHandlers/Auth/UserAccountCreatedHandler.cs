using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.Auth;

/// <summary>
/// Handles UserAccountCreatedIntegrationEvent to send welcome emails.
/// </summary>
public class UserAccountCreatedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<UserAccountCreatedHandler> logger) : NotificationEventHandler<UserAccountCreatedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(UserAccountCreatedIntegrationEvent notification)
    {
        return (notification.UserId, "email.system.welcome");
    }

    protected override async Task ProcessNotificationAsync(
        UserAccountCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate welcome email content
        // TODO: Get full name from user profile if available
        var fullName = notification.Email; // Use email as fallback name

        // Use ApplicationUrlService for proper URL generation
        var loginUrl = applicationUrlService.GetLoginUrl();

        // Only include password if it's a temporary password
        // Otherwise, the user set their own password and shouldn't see it in email
        var passwordToShow = notification.IsTemporaryPassword ? notification.TemporaryPassword : null;

        // Use Identification for login credential display, fallback to email if not provided
        var loginCredential = !string.IsNullOrWhiteSpace(notification.Identification)
            ? notification.Identification
            : notification.Email;

        // Generate confirmation link if token is provided
        string? confirmationUrl = null;
        if (!string.IsNullOrWhiteSpace(notification.EmailConfirmationToken))
        {
            confirmationUrl = applicationUrlService.GetEmailConfirmationUrl(
                notification.UserId,
                notification.EmailConfirmationToken);
        }

        var emailBody = emailTemplateService.GenerateWelcomeEmail(
            fullName,
            loginCredential, // Pass identification instead of email for login
            passwordToShow, // Pass null if not temporary
            loginUrl,
            notification.EmailConfirmed,
            confirmationUrl);

        // Queue the email
        emailQueueService.QueueEmail(
            notification.Email,
            "Bienvenido al Sistema",
            emailBody);

        // Logging handled by base class
        await Task.CompletedTask; // Keep async signature for consistency
    }
}
