using LinaSys.Auth.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.Auth;

/// <summary>
/// Handles EmailChangeRequestedIntegrationEvent to send email verification emails.
/// </summary>
public class EmailChangeRequestedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<EmailChangeRequestedHandler> logger) : NotificationEventHandler<EmailChangeRequestedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(EmailChangeRequestedIntegrationEvent notification)
    {
        return (notification.UserId, "email.account.changes");
    }

    protected override async Task ProcessNotificationAsync(
        EmailChangeRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate email change verification content
        var verificationLink = applicationUrlService.GetEmailChangeVerificationUrl(notification.VerificationToken);

        var emailBody = emailTemplateService.GenerateEmailChangeVerificationEmail(
            notification.OldEmail, // Use old email as name for now
            notification.OldEmail,
            notification.NewEmail,
            verificationLink);

        // Queue verification email to the NEW email address
        emailQueueService.QueueEmail(
            notification.NewEmail,
            "Verificación de Cambio de Correo Electrónico",
            emailBody);

        // Optionally send notification to current email as well for security
        // This could be a different template informing about the change request
        await Task.CompletedTask; // Keep async signature for consistency
    }
}