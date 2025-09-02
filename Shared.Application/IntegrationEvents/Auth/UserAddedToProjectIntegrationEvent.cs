using MediatR;

namespace LinaSys.Shared.Application.IntegrationEvents.Auth;

public record UserAddedToProjectIntegrationEvent(
    string UserId,
    string UserEmail,
    string UserName,
    long ProjectId,
    string ProjectName,
    long IncubatorId,
    string Role,
    DateTime OccurredAt) : INotification;
