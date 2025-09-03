using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Domain.IntegrationEvents;

public record FormRejectedIntegrationEvent(
    int SubmissionId,
    int ProjectId,
    string ProjectName,
    string ParticipantUserId,
    string ParticipantEmail,
    string ParticipantName,
    string Feedback,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}