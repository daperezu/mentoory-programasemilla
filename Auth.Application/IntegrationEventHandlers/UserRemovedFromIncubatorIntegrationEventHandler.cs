using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers;

/// <summary>
/// Handles the UserRemovedFromIncubatorIntegrationEvent by deactivating UserIncubatorAccess.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserRemovedFromIncubatorIntegrationEventHandler"/> class.
/// </remarks>
/// <param name="repository">The auth repository.</param>
/// <param name="timeProvider">The time provider service.</param>
/// <param name="logger">The logger.</param>
public class UserRemovedFromIncubatorIntegrationEventHandler(
    IAuthRepository repository,
    ITimeProvider timeProvider,
    ILogger<UserRemovedFromIncubatorIntegrationEventHandler> logger) : INotificationHandler<UserRemovedFromIncubatorIntegrationEvent>
{

    /// <inheritdoc/>
    public async Task Handle(UserRemovedFromIncubatorIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogWarning("Received null UserRemovedFromIncubatorIntegrationEvent");
            return;
        }

        try
        {
            // Find the user's access to this incubator
            var existingAccess = await repository.GetUserIncubatorAccessAsync(
                notification.UserId,
                notification.IncubatorId,
                cancellationToken);

            if (existingAccess is null)
            {
                logger.LogWarning(
                    "No access record found for user {UserId} in incubator {IncubatorId} to remove",
                    notification.UserId,
                    notification.IncubatorId);
                return;
            }

            if (!existingAccess.IsActive)
            {
                // Already deactivated - idempotent
                logger.LogDebug(
                    "User {UserId} access to incubator {IncubatorId} is already deactivated",
                    notification.UserId,
                    notification.IncubatorId);
                return;
            }

            // Deactivate the access
            existingAccess.Deactivate(timeProvider.UtcNow);

            logger.LogInformation(
                "Deactivated access for user {UserId} in incubator {IncubatorId}",
                notification.UserId,
                notification.IncubatorId);

            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling UserRemovedFromIncubatorIntegrationEvent for user {UserId} and incubator {IncubatorId}",
                notification.UserId,
                notification.IncubatorId);
            throw;
        }
    }
}