using System;
using System.Threading;
using System.Threading.Tasks;
using LinaSys.Auth.Domain.AggregatesModel.Access;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers
{
    /// <summary>
    /// Handles the UserAddedToIncubatorIntegrationEvent by creating or updating UserIncubatorAccess.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserAddedToIncubatorIntegrationEventHandler"/> class.
    /// </remarks>
    /// <param name="repository">The auth repository.</param>
    /// <param name="timeProvider">The time provider service.</param>
    /// <param name="logger">The logger.</param>
    public class UserAddedToIncubatorIntegrationEventHandler(
        IAuthRepository repository,
        ITimeProvider timeProvider,
        ILogger<UserAddedToIncubatorIntegrationEventHandler> logger) : INotificationHandler<UserAddedToIncubatorIntegrationEvent>
    {

        /// <inheritdoc/>
        public async Task Handle(UserAddedToIncubatorIntegrationEvent notification, CancellationToken cancellationToken)
        {
            if (notification is null)
            {
                logger.LogWarning("Received null UserAddedToIncubatorIntegrationEvent");
                return;
            }

            try
            {
                // Check if user already has access to this incubator
                var existingAccess = await repository.GetUserIncubatorAccessAsync(
                    notification.UserId,
                    notification.IncubatorId,
                    cancellationToken);

                if (existingAccess is not null)
                {
                    // If access exists but is inactive, reactivate it
                    if (!existingAccess.IsActive)
                    {
                        existingAccess.Reactivate(notification.Role, timeProvider.UtcNow);
                        logger.LogInformation(
                            "Reactivated incubator access for user {UserId} in incubator {IncubatorId} with role {Role}",
                            notification.UserId,
                            notification.IncubatorId,
                            notification.Role);
                    }
                    else if (existingAccess.Role != notification.Role)
                    {
                        // Update role if different
                        existingAccess.UpdateRole(notification.Role, timeProvider.UtcNow);
                        logger.LogInformation(
                            "Updated role for user {UserId} in incubator {IncubatorId} to {Role}",
                            notification.UserId,
                            notification.IncubatorId,
                            notification.Role);
                    }
                    else
                    {
                        // Access already exists and is active with same role - idempotent
                        logger.LogDebug(
                            "User {UserId} already has active access to incubator {IncubatorId} with role {Role}",
                            notification.UserId,
                            notification.IncubatorId,
                            notification.Role);
                        return;
                    }
                }
                else
                {
                    // Create new access record
                    var newAccess = UserIncubatorAccess.Create(
                        notification.UserId,
                        notification.IncubatorId,
                        notification.Role,
                        timeProvider.UtcNow);

                    await repository.AddUserIncubatorAccessAsync(newAccess, cancellationToken);
                    logger.LogInformation(
                        "Created incubator access for user {UserId} in incubator {IncubatorId} with role {Role}",
                        notification.UserId,
                        notification.IncubatorId,
                        notification.Role);
                }

                await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error handling UserAddedToIncubatorIntegrationEvent for user {UserId} and incubator {IncubatorId}",
                    notification.UserId,
                    notification.IncubatorId);
                throw;
            }
        }
    }
}
