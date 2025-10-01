using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.BusinessIncubator;

/// <summary>
/// Integration event raised when a project stage is activated.
/// Used to notify participants about new forms becoming available.
/// </summary>
public record ProjectStageActivatedIntegrationEvent(
    long ProjectId,
    Guid ProjectExternalId,
    string ProjectName,
    string StageType,
    string Phase,
    string StageName,
    DateTime StartDate,
    DateTime EndDate,
    List<ParticipantNotificationInfo> Participants,
    DateTime OccurredAt) : INotification;

/// <summary>
/// Information about a participant to be notified.
/// </summary>
public record ParticipantNotificationInfo(
    string UserId,
    string Email,
    string FullName);