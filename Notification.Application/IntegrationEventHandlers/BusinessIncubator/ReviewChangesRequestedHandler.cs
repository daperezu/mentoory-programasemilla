using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using LinaSys.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.BusinessIncubator;

/// <summary>
/// Handles ReviewChangesRequestedIntegrationEvent to send review feedback notification emails.
/// </summary>
public class ReviewChangesRequestedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    IApplicationUrlService applicationUrlService,
    ILogger<ReviewChangesRequestedHandler> logger) : NotificationEventHandler<ReviewChangesRequestedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(ReviewChangesRequestedIntegrationEvent notification)
    {
        return (notification.ParticipantUserId, "email.form.submissions");
    }

    protected override async Task ProcessNotificationAsync(
        ReviewChangesRequestedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate the form edit URL with complete project context
        var formEditUrl = applicationUrlService.GetFormEditUrl(notification.ProjectId, notification.SubmissionId);

        // Build the feedback message with deadline if provided
        var feedbackMessage = $"El revisor {notification.ReviewerName} ha solicitado los siguientes cambios:\n\n{notification.Feedback}";

        if (notification.NewDeadline.HasValue)
        {
            feedbackMessage += $"\n\nNueva fecha límite: {notification.NewDeadline.Value:dd/MM/yyyy}";
        }

        // Generate email with complete information
        var emailBody = emailTemplateService.GenerateFormRejectedEmail(
            notification.ParticipantName,
            notification.ProjectName,
            feedbackMessage,
            formEditUrl);

        // Queue review feedback notification to participant
        emailQueueService.QueueEmail(
            notification.ParticipantEmail,
            $"Cambios Solicitados en tu Formulario - {notification.ProjectName}",
            emailBody);

        await Task.CompletedTask; // Keep async signature for consistency
    }
}