using LinaSys.Shared.Application.IntegrationEvents;

namespace LinaSys.BusinessIncubator.Domain.IntegrationEvents;

public record UserInvitedToProjectIntegrationEvent(
    int ProjectId,
    string ProjectName,
    string InvitedUserId,
    string InvitedEmail,
    string InviterName,
    string InvitationUrl,
    DateTime OccurredOn) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    DateTime IIntegrationEvent.OccurredOn => OccurredOn;
}