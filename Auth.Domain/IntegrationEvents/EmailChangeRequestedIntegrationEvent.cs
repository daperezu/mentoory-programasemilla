using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.Auth.Domain.IntegrationEvents;

public record EmailChangeRequestedIntegrationEvent(
    string UserId,
    string OldEmail,
    string NewEmail,
    string VerificationToken,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}