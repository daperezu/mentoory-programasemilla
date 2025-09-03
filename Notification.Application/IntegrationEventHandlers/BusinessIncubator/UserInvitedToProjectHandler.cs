using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.BusinessIncubator;

/// <summary>
/// Handles UserInvitedToProjectIntegrationEvent to send project invitation emails.
/// </summary>
public class UserInvitedToProjectHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    ILogger<UserInvitedToProjectHandler> logger) : NotificationEventHandler<UserInvitedToProjectIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(UserInvitedToProjectIntegrationEvent notification)
    {
        return (notification.InvitedUserId, "email.invitations");
    }

    protected override async Task ProcessNotificationAsync(
        UserInvitedToProjectIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // For simplicity, we'll use the project invitation template for all invitations
        // The InvitationUrl should contain the appropriate link (activation or acceptance)
        var emailBody = emailTemplateService.GenerateProjectInvitationEmail(
            notification.InvitedEmail, // Use email as name for now
            notification.ProjectName,
            notification.InvitationUrl);

        var subject = $"Invitación a {notification.ProjectName} de {notification.InviterName}";

        // Queue invitation email
        emailQueueService.QueueEmail(
            notification.InvitedEmail,
            subject,
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }
}