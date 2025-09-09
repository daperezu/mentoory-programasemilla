using System;
using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth
{
    /// <summary>
    /// Integration event raised when a mentor is assigned to a starter.
    /// Used by Auth domain to update mentorship access read models.
    /// </summary>
    public record MentorAssignedIntegrationEvent(
        string MentorUserId,
        string StarterUserId,
        long ProjectId,
        long IncubatorId,
        DateTime AssignedAt) : INotification;
}