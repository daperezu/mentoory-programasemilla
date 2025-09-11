using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionById;

/// <summary>
/// Query to get a submission by its ID.
/// </summary>
public record GetSubmissionByIdQuery(long SubmissionId) : IBaseRequest<ProjectFormSubmissionDto?>;

/// <summary>
/// DTO for project form submission.
/// </summary>
public class ProjectFormSubmissionDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the participant user ID.
    /// </summary>
    public string ParticipantUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the coordinator user ID.
    /// </summary>
    public string? CoordinatorUserId { get; set; }

    /// <summary>
    /// Gets or sets the coordinator data.
    /// </summary>
    public string? CoordinatorData { get; set; }

    /// <summary>
    /// Gets or sets the coordinator reviewed at date.
    /// </summary>
    public DateTime? CoordinatorReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the draft data.
    /// </summary>
    public string DraftData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phase.
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submitted at date.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the approved at date.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// Handler for GetSubmissionByIdQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetSubmissionByIdQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
public class GetSubmissionByIdQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetSubmissionByIdQuery, ProjectFormSubmissionDto?>
{
    /// <inheritdoc/>
    public override async Task<Result<ProjectFormSubmissionDto?>> Handle(
        GetSubmissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var submission = await repository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Success(null);
        }

        var dto = new ProjectFormSubmissionDto
        {
            Id = submission.Id,
            ProjectId = submission.ProjectId,
            ParticipantUserId = submission.ParticipantUserId,
            CoordinatorUserId = submission.CoordinatorUserId,
            CoordinatorData = submission.CoordinatorData,
            CoordinatorReviewedAt = submission.CoordinatorReviewedAt,
            DraftData = submission.DraftData ?? string.Empty,
            Status = submission.Status.ToString(),
            Phase = submission.Phase.ToString(),
            SubmittedAt = submission.SubmittedAt,
            ApprovedAt = submission.ApprovedAt
        };

        return Success(dto);
    }
}