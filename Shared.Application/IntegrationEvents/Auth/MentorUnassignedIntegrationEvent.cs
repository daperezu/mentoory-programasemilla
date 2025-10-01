using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth;

/// <summary>
/// Integration event raised when a mentor is unassigned from a starter.
/// Used by Auth domain to update mentorship access read models.
/// </summary>
public record MentorUnassignedIntegrationEvent(
    string MentorUserId,
    string StarterUserId,
    long ProjectId,
    DateTime UnassignedAt) : INotification;