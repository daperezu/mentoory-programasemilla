using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.UserManagement.Domain.IntegrationEvents;

public record UserProfileUpdatedIntegrationEvent(
    string UserId,
    Dictionary<string, object> Changes,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}