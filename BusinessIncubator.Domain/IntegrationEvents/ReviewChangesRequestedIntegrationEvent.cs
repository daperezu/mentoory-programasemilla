using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Domain.IntegrationEvents;

public record ReviewChangesRequestedIntegrationEvent(
    long SubmissionId,
    long ProjectId,
    string ProjectName,
    string ParticipantUserId,
    string ParticipantName,
    string ParticipantEmail,
    string ReviewerName,
    string Feedback,
    DateTime? NewDeadline,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}