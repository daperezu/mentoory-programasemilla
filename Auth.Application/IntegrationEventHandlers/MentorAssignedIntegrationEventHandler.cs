using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the MentorAssignedIntegrationEvent by creating or updating UserMentorshipAccess.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MentorAssignedIntegrationEventHandler"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="timeProvider">The time provider service.</param>
/// <param name="logger">The logger.</param>
public class MentorAssignedIntegrationEventHandler(
    IAuthRepository repository,
    ITimeProvider timeProvider,
    ILogger<MentorAssignedIntegrationEventHandler> logger) : INotificationHandler<MentorAssignedIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(MentorAssignedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Check if mentorship already exists for this starter in this project
            var existingMentorship = await repository.GetActiveMentorshipAccessByStarterAsync(
                notification.StarterUserId,
                notification.ProjectId,
                cancellationToken);

            if (existingMentorship is not null)
            {
                // If mentorship exists but is inactive or with different mentor, update it
                if (!existingMentorship.IsActive)
                {
                    // Reactivate with potentially new mentor
                    existingMentorship.Reactivate(timeProvider.UtcNow);
                    logger.LogInformation(
                        "Reactivated mentorship for starter {StarterUserId} with mentor {MentorUserId} in project {ProjectId}",
                        notification.StarterUserId,
                        notification.MentorUserId,
                        notification.ProjectId);
                }
                else if (existingMentorship.MentorUserId != notification.MentorUserId)
                {
                    // End current mentorship and create new one
                    existingMentorship.End(timeProvider.UtcNow);

                    var newMentorship = UserMentorshipAccess.Create(
                        notification.MentorUserId,
                        notification.StarterUserId,
                        notification.ProjectId,
                        notification.IncubatorId,
                        timeProvider.UtcNow);

                    await repository.AddUserMentorshipAccessAsync(newMentorship, cancellationToken);
                    logger.LogInformation(
                        "Changed mentor for starter {StarterUserId} from {OldMentorUserId} to {NewMentorUserId} in project {ProjectId}",
                        notification.StarterUserId,
                        existingMentorship.MentorUserId,
                        notification.MentorUserId,
                        notification.ProjectId);
                }
                else
                {
                    // Same mentor already assigned and active - idempotent
                    logger.LogDebug(
                        "Mentorship already active for starter {StarterUserId} with mentor {MentorUserId} in project {ProjectId}",
                        notification.StarterUserId,
                        notification.MentorUserId,
                        notification.ProjectId);
                    return;
                }
            }
            else
            {
                // Create new mentorship record
                var newMentorship = UserMentorshipAccess.Create(
                    notification.MentorUserId,
                    notification.StarterUserId,
                    notification.ProjectId,
                    notification.IncubatorId,
                    timeProvider.UtcNow);

                await repository.AddUserMentorshipAccessAsync(newMentorship, cancellationToken);
                logger.LogInformation(
                    "Created mentorship for starter {StarterUserId} with mentor {MentorUserId} in project {ProjectId}",
                    notification.StarterUserId,
                    notification.MentorUserId,
                    notification.ProjectId);
            }

            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling MentorAssignedIntegrationEvent for starter {StarterUserId} and mentor {MentorUserId}",
                notification.StarterUserId,
                notification.MentorUserId);
            throw;
        }
    }
}
