using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth;

/// <summary>
/// Integration event raised when a user is removed from a project.
/// Used by Auth domain to update access control read models.
/// </summary>
public record UserRemovedFromProjectIntegrationEvent(
    string UserId,
    long ProjectId,
    string Reason,
    DateTime OccurredAt) : INotification;