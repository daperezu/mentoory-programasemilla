using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.BusinessIncubator;

/// <summary>
/// Handles FormApprovedIntegrationEvent to send form approval notification emails.
/// </summary>
public class FormApprovedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<FormApprovedHandler> logger) : NotificationEventHandler<FormApprovedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(FormApprovedIntegrationEvent notification)
    {
        return (notification.ParticipantUserId, "email.approvals");
    }

    protected override async Task ProcessNotificationAsync(
        FormApprovedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate form approval email
        var projectDashboardUrl = applicationUrlService.GetParticipantProjectDashboardUrl(notification.ProjectId);

        var emailBody = emailTemplateService.GenerateFormApprovedEmail(
            notification.ParticipantName,
            notification.ProjectName,
            projectDashboardUrl);

        // Queue approval notification to participant
        emailQueueService.QueueEmail(
            notification.ParticipantEmail,
            $"¡Felicitaciones! Tu solicitud ha sido aprobada - {notification.ProjectName}",
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }
}