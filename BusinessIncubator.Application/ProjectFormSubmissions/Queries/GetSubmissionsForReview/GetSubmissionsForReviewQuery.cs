using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionsForReview;

/// <summary>
/// Query to get form submissions pending review.
/// </summary>
public sealed record GetSubmissionsForReviewQuery : IBaseRequest<List<SubmissionForReviewDto>>
{
    /// <summary>
    /// Gets the project external ID.
    /// </summary>
    public Guid ProjectExternalId { get; init; }

    /// <summary>
    /// Gets a value indicating whether gets whether to include only pending submissions.
    /// </summary>
    public bool OnlyPending { get; init; } = true;
}