using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.BusinessIncubator;

/// <summary>
/// Handles FormRejectedIntegrationEvent to send form rejection notification emails.
/// </summary>
public class FormRejectedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<FormRejectedHandler> logger) : NotificationEventHandler<FormRejectedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(FormRejectedIntegrationEvent notification)
    {
        return (notification.ParticipantUserId, "email.rejections");
    }

    protected override async Task ProcessNotificationAsync(
        FormRejectedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate form rejection email
        var formEditUrl = applicationUrlService.GetFormEditUrl(notification.ProjectId, notification.SubmissionId);

        var emailBody = emailTemplateService.GenerateFormRejectedEmail(
            notification.ParticipantName,
            notification.ProjectName,
            notification.Feedback, // Use Feedback instead of RejectionReason
            formEditUrl);

        // Queue rejection notification to participant
        emailQueueService.QueueEmail(
            notification.ParticipantEmail,
            $"Solicitud Requiere Cambios - {notification.ProjectName}",
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }
}