using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents;

/// <summary>
/// Marker interface for integration events that can be published across modules or services.
/// Integration events represent something that has already happened in the domain.
/// </summary>
public interface IIntegrationEvent : INotification
{
    /// <summary>
    /// Unique identifier for this integration event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The time when this event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
