using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when a project invitation is accepted.
/// </summary>
/// <param name="ProjectId">The project ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="UserId">The user ID of the participant.</param>
/// <param name="UserEmail">The email of the participant.</param>
/// <param name="UserFullName">The full name of the participant.</param>
/// <param name="IdentificationNumber">The identification number of the participant.</param>
/// <param name="Role">The role assigned to the participant (optional).</param>
/// <param name="AcceptedAt">The timestamp when the invitation was accepted.</param>
public sealed record ProjectInvitationAccepted(
    long ProjectId,
    Guid ProjectExternalId,
    string UserId,
    string UserEmail,
    string UserFullName,
    string IdentificationNumber,
    string? Role,
    DateTime AcceptedAt) : IntegrationEvent, INotification;