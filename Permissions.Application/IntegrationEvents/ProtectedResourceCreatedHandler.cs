using LinaSys.Permissions.Application.ProtectedResource.Commands;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Permissions.Application.IntegrationEvents;

/// <summary>
/// Handler for the <see cref="ProtectedResourceCreated"/> integration event.
/// This handler creates the actual protected resource and assigns permissions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProtectedResourceCreatedHandler"/> class.
/// </remarks>
/// <param name="mediator">The mediator instance.</param>
/// <param name="logger">The logger instance.</param>
public class ProtectedResourceCreatedHandler(IMediator mediator, ILogger<ProtectedResourceCreatedHandler> logger) : INotificationHandler<ProtectedResourceCreated>
{

    /// <inheritdoc />
    public async Task Handle(ProtectedResourceCreated notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Handling ProtectedResourceCreated event for resource {ExternalId} of type {ResourceType}",
                notification.ExternalId,
                notification.ResourceType);

            var createProtectedResourceCommand = new CreateProtectedResourceCommand(
                notification.ExternalId,
                notification.ResourceType,
                notification.Name,
                notification.CreatorUserId);

            var result = await mediator.Send(createProtectedResourceCommand, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Successfully created protected resource {ExternalId} with internal ID {InternalId}",
                    notification.ExternalId,
                    result.Value);
            }
            else
            {
                logger.LogWarning(
                    "Failed to create protected resource {ExternalId}: {ErrorCode} - {ErrorMessages}",
                    notification.ExternalId,
                    result.ErrorCode,
                    string.Join(", ", result.ErrorMessages?.Select(e => e.Message) ?? Enumerable.Empty<string>()));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProtectedResourceCreated event for resource {ExternalId}",
                notification.ExternalId);

            #if DEBUG
            // In debug mode (tests), rethrow to help identify issues
            throw;
            #endif

            // In a production system, you might want to implement retry logic or dead letter queues
            // For now, we'll just log the error and continue
        }
    }
}
