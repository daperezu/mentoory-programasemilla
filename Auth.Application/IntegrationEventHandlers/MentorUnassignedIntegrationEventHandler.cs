using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the MentorUnassignedIntegrationEvent by ending UserMentorshipAccess.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MentorUnassignedIntegrationEventHandler"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="timeProvider">The time provider service.</param>
/// <param name="logger">The logger.</param>
public class MentorUnassignedIntegrationEventHandler(
    IAuthRepository repository,
    ITimeProvider timeProvider,
    ILogger<MentorUnassignedIntegrationEventHandler> logger) : INotificationHandler<MentorUnassignedIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(MentorUnassignedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogWarning("Received null MentorUnassignedIntegrationEvent");
            return;
        }

        try
        {
            // Find the active mentorship relationship
            var existingMentorship = await repository.GetMentorshipAccessAsync(
                notification.MentorUserId,
                notification.StarterUserId,
                notification.ProjectId,
                cancellationToken);

            if (existingMentorship is null || !existingMentorship.IsActive)
            {
                var message = existingMentorship is null
                    ? "No mentorship found"
                    : "Mentorship is already ended";

                logger.LogWarning(
                    "{Message} between mentor {MentorUserId} and starter {StarterUserId} in project {ProjectId}",
                    message,
                    notification.MentorUserId,
                    notification.StarterUserId,
                    notification.ProjectId);
                return;
            }

            // End the mentorship
            existingMentorship.End(timeProvider.UtcNow);

            logger.LogInformation(
                "Ended mentorship between mentor {MentorUserId} and starter {StarterUserId} in project {ProjectId}",
                notification.MentorUserId,
                notification.StarterUserId,
                notification.ProjectId);

            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling MentorUnassignedIntegrationEvent for mentor {MentorUserId} and starter {StarterUserId}",
                notification.MentorUserId,
                notification.StarterUserId);
            throw;
        }
    }
}