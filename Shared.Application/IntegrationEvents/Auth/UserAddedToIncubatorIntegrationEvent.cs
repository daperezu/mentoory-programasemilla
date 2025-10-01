using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth;

/// <summary>
/// Integration event raised when a user is added to an incubator.
/// Used by Auth domain to update incubator access read models.
/// </summary>
public record UserAddedToIncubatorIntegrationEvent(
    string UserId,
    string UserEmail,
    string UserName,
    long IncubatorId,
    string IncubatorName,
    string Role,
    DateTime OccurredAt) : INotification;