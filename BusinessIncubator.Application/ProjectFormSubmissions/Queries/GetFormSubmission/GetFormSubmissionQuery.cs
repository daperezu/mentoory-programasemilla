using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;

/// <summary>
/// Query to get a form submission details.
/// </summary>
public sealed record GetFormSubmissionQuery : IBaseRequest<FormSubmissionDto>
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