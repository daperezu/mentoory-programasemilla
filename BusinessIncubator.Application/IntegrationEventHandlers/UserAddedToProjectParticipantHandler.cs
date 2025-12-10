using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.IntegrationEvents.Auth;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.IntegrationEventHandlers;

/// <summary>
/// Inline implementation of IAuditContext for integration event handlers.
/// </summary>
internal record InlineAuditContext(string? User, DateTime UtcNow) : IAuditContext;

/// <summary>
/// Handles the UserAddedToProjectIntegrationEvent by creating or updating ProjectUser records.
/// This ensures users assigned to projects are registered as participants in the BusinessIncubator domain.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserAddedToProjectParticipantHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="timeProvider">The time provider service.</param>
/// <param name="logger">The logger.</param>
public sealed class UserAddedToProjectParticipantHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider,
    ILogger<UserAddedToProjectParticipantHandler> logger) : INotificationHandler<UserAddedToProjectIntegrationEvent>
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
            logger.LogInformation(
                "Processing UserAddedToProjectIntegrationEvent to create ProjectUser for user {UserId} in project {ProjectId}",
                notification.UserId,
                notification.ProjectId);

            // Fetch the project to add the user
            var project = await repository.GetProjectByIdAsync(notification.ProjectId, cancellationToken);

            if (project is null)
            {
                logger.LogWarning(
                    "Project {ProjectId} not found. Cannot create ProjectUser for user {UserId}",
                    notification.ProjectId,
                    notification.UserId);
                return; // Graceful degradation - don't fail the operation
            }

            if (project.IsDeleted)
            {
                logger.LogWarning(
                    "Project {ProjectId} is deleted. Skipping ProjectUser creation for user {UserId}",
                    notification.ProjectId,
                    notification.UserId);
                return;
            }

            // Create audit context for domain operation
            // Note: Integration event handlers don't have HTTP context, so we create inline
            var auditContext = new InlineAuditContext(
                User: notification.UserId, // User who was added to project
                UtcNow: timeProvider.UtcNow);

            // Check if user already exists in project
            var existingUser = project.GetUser(notification.UserId);

            if (existingUser is not null)
            {
                // User already exists
                if (existingUser.IsActive)
                {
                    // User is already active - check if role needs update
                    if (existingUser.Role != notification.Role)
                    {
                        // Role changed - we don't have an UpdateRole method, so log warning
                        logger.LogWarning(
                            "User {UserId} already active in project {ProjectId} with role {CurrentRole}, but event specifies role {NewRole}. Manual intervention may be needed.",
                            notification.UserId,
                            notification.ProjectId,
                            existingUser.Role,
                            notification.Role);
                    }
                    else
                    {
                        logger.LogDebug(
                            "User {UserId} already has active participation in project {ProjectId} with role {Role}. Idempotent operation.",
                            notification.UserId,
                            notification.ProjectId,
                            notification.Role);
                    }

                    return; // Idempotent - no action needed
                }
                else
                {
                    // User exists but inactive - will be reactivated by AddUser
                    logger.LogInformation(
                        "User {UserId} exists but is inactive in project {ProjectId}. Will reactivate with role {Role}.",
                        notification.UserId,
                        notification.ProjectId,
                        notification.Role);
                }
            }

            // Add user to project (handles both new users and reactivation)
            try
            {
                project.AddUser(
                    userId: notification.UserId,
                    role: notification.Role,
                    invitedBy: notification.UserId, // For now, assume self-invited (can be enhanced later)
                    auditContext: auditContext);

                // Save changes
                await repository.UpdateAsync(project, cancellationToken);
                await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Successfully created/reactivated ProjectUser for user {UserId} in project {ProjectId} ({ProjectName}) with role {Role}",
                    notification.UserId,
                    notification.ProjectId,
                    notification.ProjectName,
                    notification.Role);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already active"))
            {
                // Concurrent creation - another handler already added the user
                logger.LogDebug(
                    "User {UserId} was concurrently added to project {ProjectId}. Idempotent operation.",
                    notification.UserId,
                    notification.ProjectId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling UserAddedToProjectIntegrationEvent for user {UserId} and project {ProjectId}. ProjectUser may not have been created.",
                notification.UserId,
                notification.ProjectId);

            // DON'T throw - graceful degradation
            // The user still has Auth domain access and form submissions
            // ProjectUser can be manually created if needed
        }
    }
}
