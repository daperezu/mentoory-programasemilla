using LinaSys.BusinessIncubator.Domain.IntegrationEvents;
using LinaSys.Notification.Application.Interfaces;
using LinaSys.Notification.Application.Services;
using LinaSys.Notification.Application.Templates;
using Microsoft.Extensions.Logging;

namespace LinaSys.Notification.Application.IntegrationEventHandlers.BusinessIncubator;

/// <summary>
/// Handles FormSubmittedIntegrationEvent to send form submission confirmation emails.
/// </summary>
public class FormSubmittedHandler(
    IEmailPreferenceService emailPreferenceService,
    IEmailQueueService emailQueueService,
    IEmailTemplateService emailTemplateService,
    ILogger<FormSubmittedHandler> logger) : NotificationEventHandler<FormSubmittedIntegrationEvent>(emailPreferenceService, logger)
{
    protected override (string UserId, string PreferenceKey) GetUserAndPreferenceKey(FormSubmittedIntegrationEvent notification)
    {
        return (notification.ParticipantUserId, "email.form.submissions");
    }

    protected override async Task ProcessNotificationAsync(
        FormSubmittedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        // Generate form submission confirmation email
        var submissionDateTime = notification.SubmittedAt.ToString("dd/MM/yyyy HH:mm");
        var submissionId = $"SUB-{notification.SubmissionId:D6}";

        var emailBody = emailTemplateService.GenerateFormSubmissionConfirmationEmail(
            notification.ParticipantName,
            notification.ProjectName,
            submissionDateTime,
            submissionId);

        // Queue confirmation email to participant
        emailQueueService.QueueEmail(
            notification.ParticipantEmail,
            $"Confirmación de Envío - {notification.ProjectName}",
            emailBody);

        // TODO: Also send notification to administrators/reviewers
        // This might be a separate event or handled differently
        await Task.CompletedTask; // Keep async signature for consistency
    }
}