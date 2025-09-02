using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Shared.Application.IntegrationEvents;

/// <summary>
/// MediatR-based implementation of <see cref="IIntegrationEventService"/>.
/// This implementation publishes integration events as notifications using MediatR.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MediatRIntegrationEventService"/> class.
/// </remarks>
/// <param name="publisher">The MediatR publisher.</param>
/// <param name="logger">The logger.</param>
public class MediatRIntegrationEventService(IPublisher publisher, ILogger<MediatRIntegrationEventService> logger) : IIntegrationEventService
{

    /// <inheritdoc />
    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Publishing integration event {EventType} with ID {EventId}",
                integrationEvent.GetType().Name,
                integrationEvent.EventId);

            await publisher.Publish(integrationEvent, cancellationToken);

            logger.LogInformation(
                "Successfully published integration event {EventType} with ID {EventId}",
                integrationEvent.GetType().Name,
                integrationEvent.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish integration event {EventType} with ID {EventId}",
                integrationEvent.GetType().Name,
                integrationEvent.EventId);

            // In a production system, you might want to store failed events for retry
            // For now, we'll just log the error and continue
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default)
    {
        foreach (var integrationEvent in integrationEvents)
        {
            await PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
