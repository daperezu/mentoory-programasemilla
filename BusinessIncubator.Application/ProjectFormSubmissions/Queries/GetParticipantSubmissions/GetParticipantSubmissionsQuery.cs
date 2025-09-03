using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;

/// <summary>
/// Query to get all form submissions for a specific participant.
/// </summary>
public sealed record GetParticipantSubmissionsQuery : IBaseRequest<List<ParticipantSubmissionDto>>
{
    /// <summary>
    /// Gets the project external ID.
    /// </summary>
    public Guid ProjectExternalId { get; init; }

    /// <summary>
    /// Gets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; init; }
}