using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.Auth.Domain.IntegrationEvents;

public record UserAccountCreatedIntegrationEvent(
    string UserId,
    string Email,
    string? Identification,
    string TemporaryPassword,
    DateTime CreatedAt,
    DateTime OccurredOn,
    bool EmailConfirmed = false,
    bool IsTemporaryPassword = false,
    string? EmailConfirmationToken = null) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}