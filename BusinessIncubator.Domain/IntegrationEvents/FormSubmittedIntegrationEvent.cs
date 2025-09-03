using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Domain.IntegrationEvents;

public record FormSubmittedIntegrationEvent(
    int SubmissionId,
    int ProjectId,
    string ProjectName,
    string ParticipantUserId,
    string ParticipantEmail,
    string ParticipantName,
    DateTime SubmittedAt,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}