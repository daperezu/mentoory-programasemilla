using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.Auth;

/// <summary>
/// Handles PasswordResetRequestedIntegrationEvent to send password reset emails.
/// </summary>
public class PasswordResetRequestedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<PasswordResetRequestedHandler> logger) : NotificationEventHandler<PasswordResetRequestedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(PasswordResetRequestedIntegrationEvent notification)
    {
        return (notification.UserId, "email.password.reset");
    }

    protected override async Task ProcessNotificationAsync(
        PasswordResetRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate password reset email content
        // Encode token for URL
        var encodedToken = System.Net.WebUtility.UrlEncode(notification.ResetToken);
        var resetLink = applicationUrlService.GetPasswordResetUrl(encodedToken);
        var requestDateTime = notification.OccurredOn.ToString("dd/MM/yyyy HH:mm");
        var requestLocation = "Sistema LinaSys"; // TODO: Get IP from context if needed

        var emailBody = emailTemplateService.GeneratePasswordResetEmail(
            notification.Email, // Use email as name for now
            notification.Email,
            resetLink,
            requestDateTime,
            requestLocation);

        // Queue the email
        emailQueueService.QueueEmail(
            notification.Email,
            "Restablecimiento de Contraseña",
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }
}