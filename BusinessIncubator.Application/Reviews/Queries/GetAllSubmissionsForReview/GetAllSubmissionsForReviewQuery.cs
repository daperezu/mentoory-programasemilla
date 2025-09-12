using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Reviews.Queries.GetAllSubmissionsForReview;

/// <summary>
/// Query to get all submissions for review (regardless of status).
/// </summary>
public record GetAllSubmissionsForReviewQuery(
    string UserId,
    long? ProjectId,
    long? IncubatorId,
    bool IsGlobalAdmin,
    bool IsAdmin) : IBaseRequest<AllSubmissionsResultDto>;

/// <summary>
/// Result DTO for all submissions.
/// </summary>
public class AllSubmissionsResultDto
{
    /// <summary>
    /// Gets or sets the submissions grouped by status.
    /// </summary>
    public List<SubmissionDto> Submissions { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// DTO for submission item.
/// </summary>
public class SubmissionDto
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long SubmissionId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project external ID.
    /// </summary>
    public Guid ProjectExternalId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage.
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the review status.
    /// </summary>
    public string ReviewStatus { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback count.
    /// </summary>
    public int FeedbackCount { get; set; }

    /// <summary>
    /// Gets or sets the deadline if any.
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Gets or sets the phase (0 = Start, 1 = Final).
    /// </summary>
    public int Phase { get; set; }
}

/// <summary>
/// Handler for GetAllSubmissionsForReviewQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetAllSubmissionsForReviewQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The repository.</param>
/// <param name="logger">The logger.</param>
public class GetAllSubmissionsForReviewQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetAllSubmissionsForReviewQueryHandler> logger) : BaseCommandHandler<GetAllSubmissionsForReviewQuery, AllSubmissionsResultDto>
{
    /// <inheritdoc/>
    public override async Task<Result<AllSubmissionsResultDto>> Handle(
        GetAllSubmissionsForReviewQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var submissions = new List<Domain.Aggregates.BusinessIncubator.ProjectFormSubmission>();

            if (request.IsGlobalAdmin)
            {
                // Global admin can see everything
                submissions = await repository.GetAllSubmissionsAsync(cancellationToken);
            }
            else if (request is { IsAdmin: true, IncubatorId: not null })
            {
                // Admin can see all submissions in their incubator
                var incubatorProjects = await repository.GetProjectsByIncubatorIdAsync(
                    request.IncubatorId.Value,
                    cancellationToken);

                var projectIds = incubatorProjects.Select(p => p.Id).ToArray();
                if (projectIds.Any())
                {
                    submissions = await repository.GetSubmissionsByProjectIdsAsync(
                        projectIds,
                        cancellationToken);
                }
            }
            else if (request.ProjectId.HasValue)
            {
                // Coordinator can see submissions for their project
                // First verify the user has access to this project
                var userProjects = await repository.GetProjectsByUserAsync(
                    request.UserId,
                    cancellationToken);

                // GetProjectsByUserAsync already filters to only projects the user has access to
                // Just check if the requested project is in the user's accessible projects
                var hasAccess = userProjects.Any(p => p.Id == request.ProjectId.Value);

                if (hasAccess)
                {
                    submissions = await repository.GetSubmissionsByProjectIdsAsync(
                        [request.ProjectId.Value],
                        cancellationToken);
                }
            }

            // Get reviews for each submission to determine review status
            var submissionDtos = new List<SubmissionDto>();
            foreach (var submission in submissions)
            {
                // Get the latest review if any
                var latestReview = await repository.GetLatestReviewBySubmissionIdAsync(
                    submission.Id,
                    cancellationToken);

                // Get feedback count (for now set to 0, can be implemented later if needed)
                var feedbackCount = 0;

                // Determine review status based on latest review
                string reviewStatus = "Pending";
                DateTime? deadline = null;

                if (latestReview is not null)
                {
                    reviewStatus = latestReview.Status switch
                    {
                        Domain.Enums.ReviewStatus.Approved => "Approved",
                        Domain.Enums.ReviewStatus.ChangesRequested => "ChangesRequested",
                        Domain.Enums.ReviewStatus.Flagged => "Flagged",
                        _ => "Pending"
                    };
                    deadline = latestReview.NewDeadline;
                }

                // Get project name
                var project = await repository.GetProjectByIdAsync(submission.ProjectId, cancellationToken);

                submissionDtos.Add(new SubmissionDto
                {
                    SubmissionId = submission.Id,
                    ProjectId = submission.ProjectId,
                    ProjectExternalId = project?.ExternalId ?? Guid.Empty,
                    ProjectName = project?.Name ?? "Proyecto",
                    UserId = submission.ParticipantUserId,
                    UserName = "Usuario", // TODO: Get from UserManager if needed
                    UserEmail = "usuario@example.com", // TODO: Get from UserManager if needed
                    SubmittedAt = submission.SubmittedAt ?? submission.StartedAt,
                    CompletionPercentage = CalculateCompletionPercentage(submission.DraftData),
                    ReviewStatus = reviewStatus,
                    Status = submission.Status.ToString(),
                    FeedbackCount = feedbackCount,
                    Deadline = deadline,
                    Phase = (int)submission.Phase
                });
            }

            // Sort by status priority: Pending first, then ChangesRequested, then others
            submissionDtos = submissionDtos
                .OrderBy(s => s.ReviewStatus switch
                {
                    "Pending" => 0,
                    "ChangesRequested" => 1,
                    "Flagged" => 2,
                    "Approved" => 3,
                    _ => 4
                })
                .ThenByDescending(s => s.SubmittedAt)
                .ToList();

            logger.LogInformation(
                "Retrieved {Count} submissions for user {UserId}",
                submissionDtos.Count,
                request.UserId);

            return Success(new AllSubmissionsResultDto
            {
                Submissions = submissionDtos,
                TotalCount = submissionDtos.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving submissions for user {UserId}", request.UserId);
            return Failure(
                ResultErrorCodes.GenericError,
                (nameof(GetAllSubmissionsForReviewQuery), "Ocurrió un error al obtener los formularios."));
        }
    }

    private int CalculateCompletionPercentage(string? draftData)
    {
        if (string.IsNullOrEmpty(draftData))
        {
            return 0;
        }

        try
        {
            var draft = System.Text.Json.JsonSerializer.Deserialize<ProjectFormSubmissions.Commands.SaveDraft.DraftDataDto>(
                draftData,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (int)(draft?.ProgressPercentage ?? 0);
        }
        catch
        {
            return 0;
        }
    }
}