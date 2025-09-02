using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Domain.IntegrationEvents;

public record FormApprovedIntegrationEvent(
    int SubmissionId,
    int ProjectId,
    string ProjectName,
    string ParticipantUserId,
    string ParticipantEmail,
    string ParticipantName,
    string DashboardUrl,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}