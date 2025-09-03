// <copyright file="UserRemovedFromProjectIntegrationEventHandler.cs" company="LinaSys">
// Copyright (c) LinaSys. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using LinaSys.Auth.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Auth.Application.IntegrationEventHandlers
{
    /// <summary>
    /// Handles the UserRemovedFromProjectIntegrationEvent by deactivating UserProjectAccess.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserRemovedFromProjectIntegrationEventHandler"/> class.
    /// </remarks>
    /// <param name="repository">The auth repository.</param>
    /// <param name="timeProvider">The time provider service.</param>
    /// <param name="logger">The logger.</param>
    public class UserRemovedFromProjectIntegrationEventHandler(
        IAuthRepository repository,
        ITimeProvider timeProvider,
        ILogger<UserRemovedFromProjectIntegrationEventHandler> logger) : INotificationHandler<UserRemovedFromProjectIntegrationEvent>
    {

        /// <inheritdoc/>
        public async Task Handle(UserRemovedFromProjectIntegrationEvent notification, CancellationToken cancellationToken)
        {
            if (notification is null)
            {
                logger.LogWarning("Received null UserRemovedFromProjectIntegrationEvent");
                return;
            }

            try
            {
                // Find the user's access to this project
                var existingAccess = await repository.GetUserProjectAccessAsync(
                    notification.UserId,
                    notification.ProjectId,
                    cancellationToken);

                if (existingAccess is null)
                {
                    logger.LogWarning(
                        "No access record found for user {UserId} in project {ProjectId} to remove",
                        notification.UserId,
                        notification.ProjectId);
                    return;
                }

                if (!existingAccess.IsActive)
                {
                    // Already deactivated - idempotent
                    logger.LogDebug(
                        "User {UserId} access to project {ProjectId} is already deactivated",
                        notification.UserId,
                        notification.ProjectId);
                    return;
                }

                // Deactivate the access
                existingAccess.Deactivate(timeProvider.UtcNow);

                logger.LogInformation(
                    "Deactivated access for user {UserId} in project {ProjectId}",
                    notification.UserId,
                    notification.ProjectId);

                await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error handling UserRemovedFromProjectIntegrationEvent for user {UserId} and project {ProjectId}",
                    notification.UserId,
                    notification.ProjectId);
                throw;
            }
        }
    }
}