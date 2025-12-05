using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetPendingReviews;

/// <summary>
/// Query to get pending reviews for a coordinator.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetPendingReviewsQuery(
    string CoordinatorId,
    long? ProjectId,
    int PageNumber = 1,
    int PageSize = 10) : IBaseRequest<PendingReviewsResultDto>;

/// <summary>
/// Result DTO for pending reviews.
/// </summary>
public class PendingReviewsResultDto
{
    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the pending reviews.
    /// </summary>
    public List<PendingReviewDto> Reviews { get; set; } = [];
}

/// <summary>
/// DTO for pending review item.
/// </summary>
public class PendingReviewDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant email.
    /// </summary>
    public string ParticipantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant role.
    /// </summary>
    public string ParticipantRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the days since submission.
    /// </summary>
    public int DaysSinceSubmission { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority level.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this is a resubmission.
    /// </summary>
    public bool IsResubmission { get; set; }

    /// <summary>
    /// Gets or sets the previous review status if resubmission.
    /// </summary>
    public string? PreviousReviewStatus { get; set; }

    /// <summary>
    /// Gets or sets the review deadline if applicable.
    /// </summary>
    public DateTime? ReviewDeadline { get; set; }
}

/// <summary>
/// Handler for GetPendingReviewsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPendingReviewsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="auditContext">The audit context.</param>
/// <param name="logger">The logger.</param>
public class GetPendingReviewsQueryHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    ILogger<GetPendingReviewsQueryHandler> logger) : BaseCommandHandler<GetPendingReviewsQuery, PendingReviewsResultDto>
{

    /// <inheritdoc/>
    public override async Task<Result<PendingReviewsResultDto>> Handle(
        GetPendingReviewsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all projects this user has access to
            var userProjects = await repository.GetProjectsByUserAsync(
                request.CoordinatorId,
                cancellationToken);

            // Filter to only coordinator/admin roles
            var coordinatorProjects = userProjects
                .Where(p => p.ProjectUsers.Any(pu =>
                    pu.UserId == request.CoordinatorId &&
                    pu.IsActive &&
                    (pu.Role == Roles.Coordinator || pu.Role == Roles.Administrator)))
                .ToList();

            if (!coordinatorProjects.Any())
            {
                return Success(new PendingReviewsResultDto
                {
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    Reviews = []
                });
            }

            // Filter by specific project if provided
            var projectIds = request.ProjectId.HasValue
                ? [request.ProjectId.Value]
                : coordinatorProjects.Select(p => p.Id).ToArray();
            if (!projectIds.Any())
            {
                return Success(new PendingReviewsResultDto
                {
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    Reviews = []
                });
            }

            // Get pending submissions
            var pendingSubmissions = await repository.GetPendingSubmissionsForReviewAsync(
                projectIds,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var totalCount = await repository.GetPendingSubmissionsCountAsync(
                projectIds,
                cancellationToken);

            // Map to DTOs
            var reviews = new List<PendingReviewDto>();
            var currentDate = auditContext.UtcNow;

            foreach (var submission in pendingSubmissions)
            {
                var daysSinceSubmission = (int)(currentDate - (submission.SubmittedAt ?? submission.StartedAt)).TotalDays;

                // Determine priority based on days since submission
                var priority = daysSinceSubmission switch
                {
                    > 7 => "High",
                    > 3 => "Medium",
                    _ => "Low"
                };

                // Check if it's a resubmission
                var previousReview = await repository.GetLatestReviewBySubmissionIdAsync(
                    submission.Id,
                    cancellationToken);

                var isResubmission = previousReview is not null &&
                    previousReview.Status == Domain.Enums.ReviewStatus.ChangesRequested;

                reviews.Add(new PendingReviewDto
                {
                    SubmissionId = submission.Id,
                    ExternalId = Guid.NewGuid(), // TODO: Add ExternalId to ProjectFormSubmission if needed
                    ProjectId = submission.ProjectId,
                    ProjectName = "Proyecto", // TODO: Get project name if needed
                    FormName = "Formulario", // TODO: Get form name if needed
                    ParticipantName = "Participante", // TODO: Get participant name from UserManager if needed
                    ParticipantEmail = string.Empty, // TODO: Get participant email from UserManager if needed
                    ParticipantRole = Roles.Starter, // TODO: Get participant role from ProjectUser if needed
                    SubmittedAt = submission.SubmittedAt ?? submission.StartedAt,
                    DaysSinceSubmission = daysSinceSubmission,
                    CompletionPercentage = 0, // TODO: Calculate completion percentage if needed
                    Status = submission.Status.ToString(),
                    Priority = priority,
                    IsResubmission = isResubmission,
                    PreviousReviewStatus = previousReview?.Status.ToString(),
                    ReviewDeadline = previousReview?.NewDeadline
                });
            }

            // Sort by priority and date
            reviews = reviews
                .OrderByDescending(r => r.Priority == "High")
                .ThenByDescending(r => r.IsResubmission)
                .ThenBy(r => r.SubmittedAt)
                .ToList();

            logger.LogInformation(
                "Retrieved {Count} pending reviews for coordinator {CoordinatorId}",
                reviews.Count,
                request.CoordinatorId);

            return Success(new PendingReviewsResultDto
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Reviews = reviews
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving pending reviews for coordinator {CoordinatorId}", request.CoordinatorId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetPendingReviewsQuery), "Ocurrió un error al obtener las revisiones pendientes."));
        }
    }
}
