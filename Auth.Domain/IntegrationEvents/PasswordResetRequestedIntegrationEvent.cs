using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.Auth.Domain.IntegrationEvents;

public record PasswordResetRequestedIntegrationEvent(
    string UserId,
    string Email,
    string ResetToken,
    DateTime ExpiresAt,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}