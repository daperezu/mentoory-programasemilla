// Copyright (c) FERPEA DYNAMICS. All rights reserved.

using LinaSys.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LinaSys.Web.Services;

/// <summary>
/// Service for sending review notifications via SignalR.
/// </summary>
public interface IReviewNotificationService
{
    /// <summary>
    /// Notifies about a review status change.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="status">The new status.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="message">Optional custom message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NotifyReviewStatusChangeAsync(long projectId, long submissionId, string status, string userName, string? message = null);

    /// <summary>
    /// Notifies about new feedback added to a submission.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="reviewerName">The reviewer name.</param>
    /// <param name="feedbackType">The feedback type.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NotifyNewFeedbackAsync(long projectId, long submissionId, string reviewerName, string feedbackType);

    /// <summary>
    /// Notifies a user about a review assignment.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="participantName">The participant name.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NotifyReviewAssignedAsync(string userId, long submissionId, string participantName);

    /// <summary>
    /// Notifies about an approaching deadline.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="submissionId">The submission ID.</param>
    /// <param name="participantName">The participant name.</param>
    /// <param name="deadline">The deadline date.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NotifyDeadlineApproachingAsync(long projectId, long submissionId, string participantName, DateTime deadline);
}

/// <inheritdoc />
/// <summary>
/// Initializes a new instance of the <see cref="ReviewNotificationService"/> class.
/// </summary>
/// <param name="hubContext">The SignalR hub context.</param>
/// <param name="logger">The logger.</param>
public class ReviewNotificationService(
    IHubContext<ReviewNotificationHub> hubContext,
    ILogger<ReviewNotificationService> logger) : IReviewNotificationService
{

    /// <inheritdoc />
    public async Task NotifyReviewStatusChangeAsync(long projectId, long submissionId, string status, string userName, string? message = null)
    {
        var groupName = $"project-{projectId}";

        var notification = new
        {
            type = "statusChange",
            submissionId,
            status,
            userName,
            message = message ?? GetStatusChangeMessage(status, userName),
            timestamp = DateTime.UtcNow
        };

        await hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

        logger.LogInformation("Sent status change notification for submission {SubmissionId} to project {ProjectId}",
            submissionId, projectId);
    }

    /// <inheritdoc />
    public async Task NotifyNewFeedbackAsync(long projectId, long submissionId, string reviewerName, string feedbackType)
    {
        var groupName = $"project-{projectId}";

        var notification = new
        {
            type = "newFeedback",
            submissionId,
            reviewerName,
            feedbackType,
            message = $"{reviewerName} agregó retroalimentación ({feedbackType})",
            timestamp = DateTime.UtcNow
        };

        await hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

        logger.LogInformation("Sent new feedback notification for submission {SubmissionId} to project {ProjectId}",
            submissionId, projectId);
    }

    /// <inheritdoc />
    public async Task NotifyReviewAssignedAsync(string userId, long submissionId, string participantName)
    {
        var connections = ReviewNotificationHub.GetUserConnections(userId);

        if (connections.Any())
        {
            var notification = new
            {
                type = "reviewAssigned",
                submissionId,
                participantName,
                message = $"Se te ha asignado la revisión del formulario de {participantName}",
                timestamp = DateTime.UtcNow
            };

            await hubContext.Clients.Clients(connections).SendAsync("ReceiveNotification", notification);

            logger.LogInformation("Sent review assignment notification to user {UserId} for submission {SubmissionId}",
                userId, submissionId);
        }
    }

    /// <inheritdoc />
    public async Task NotifyDeadlineApproachingAsync(long projectId, long submissionId, string participantName, DateTime deadline)
    {
        var groupName = $"project-{projectId}";
        var daysRemaining = (deadline - DateTime.UtcNow).Days;

        var notification = new
        {
            type = "deadlineWarning",
            submissionId,
            participantName,
            deadline,
            daysRemaining,
            message = $"El formulario de {participantName} vence en {daysRemaining} días",
            timestamp = DateTime.UtcNow
        };

        await hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

        logger.LogInformation("Sent deadline warning for submission {SubmissionId} to project {ProjectId}",
            submissionId, projectId);
    }

    private static string GetStatusChangeMessage(string status, string userName)
    {
        return status switch
        {
            "Approved" => $"El formulario de {userName} ha sido aprobado",
            "ChangesRequested" => $"Se han solicitado cambios al formulario de {userName}",
            "Flagged" => $"El formulario de {userName} ha sido marcado para revisión especial",
            _ => $"El estado del formulario de {userName} ha cambiado"
        };
    }
}
