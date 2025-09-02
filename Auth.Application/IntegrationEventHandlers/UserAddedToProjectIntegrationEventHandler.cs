using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the UserAddedToProjectIntegrationEvent by creating or updating UserProjectAccess.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserAddedToProjectIntegrationEventHandler"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="timeProvider">The time provider service.</param>
/// <param name="logger">The logger.</param>
public class UserAddedToProjectIntegrationEventHandler(
    IAuthRepository repository,
    ITimeProvider timeProvider,
    ILogger<UserAddedToProjectIntegrationEventHandler> logger) : INotificationHandler<UserAddedToProjectIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(UserAddedToProjectIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogWarning("Received null UserAddedToProjectIntegrationEvent");
            return;
        }

        try
        {
            // Check if user already has access to this project
            var existingAccess = await repository.GetUserProjectAccessAsync(
                notification.UserId,
                notification.ProjectId,
                cancellationToken);

            if (existingAccess is not null)
            {
                // If access exists but is inactive, reactivate it
                if (!existingAccess.IsActive)
                {
                    existingAccess.Reactivate(notification.Role, timeProvider.UtcNow);
                    logger.LogInformation(
                        "Reactivated project access for user {UserId} in project {ProjectId} with role {Role}",
                        notification.UserId,
                        notification.ProjectId,
                        notification.Role);
                }
                else if (existingAccess.Role != notification.Role)
                {
                    // Update role if different
                    existingAccess.UpdateRole(notification.Role, timeProvider.UtcNow);
                    logger.LogInformation(
                        "Updated role for user {UserId} in project {ProjectId} to {Role}",
                        notification.UserId,
                        notification.ProjectId,
                        notification.Role);
                }
                else
                {
                    // Access already exists and is active with same role - idempotent
                    logger.LogDebug(
                        "User {UserId} already has active access to project {ProjectId} with role {Role}",
                        notification.UserId,
                        notification.ProjectId,
                        notification.Role);
                    return;
                }
            }
            else
            {
                // Create new access record
                var newAccess = UserProjectAccess.Create(
                    notification.UserId,
                    notification.ProjectId,
                    notification.IncubatorId,
                    notification.Role,
                    timeProvider.UtcNow);

                await repository.AddUserProjectAccessAsync(newAccess, cancellationToken);
                logger.LogInformation(
                    "Created project access for user {UserId} in project {ProjectId} with role {Role}",
                    notification.UserId,
                    notification.ProjectId,
                    notification.Role);
            }

            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling UserAddedToProjectIntegrationEvent for user {UserId} and project {ProjectId}",
                notification.UserId,
                notification.ProjectId);
            throw;
        }
    }
}
