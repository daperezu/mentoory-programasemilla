using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.AspNetCore.Identity;
using LinaSys.Auth.Domain.AggregatesModel.User;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorPendingReviews;

/// <summary>
/// Query to get pending reviews for a coordinator's project.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetCoordinatorPendingReviewsQuery(long ProjectId) : IBaseRequest<CoordinatorPendingReviewsDto>;

/// <summary>
/// DTO for coordinator pending reviews.
/// </summary>
public class CoordinatorPendingReviewsDto
{
    /// <summary>
    /// Gets or sets the total number of pending reviews.
    /// </summary>
    public int TotalPending { get; set; }

    /// <summary>
    /// Gets or sets the list of pending reviews.
    /// </summary>
    public List<PendingReviewItem> Reviews { get; set; } = [];

    /// <summary>
    /// Gets or sets the oldest waiting days.
    /// </summary>
    public int OldestWaitingDays { get; set; }

    /// <summary>
    /// Gets or sets the average waiting days.
    /// </summary>
    public double AverageWaitingDays { get; set; }
}

/// <summary>
/// Individual pending review item.
/// </summary>
public class PendingReviewItem
{
    /// <summary>
    /// Gets or sets the submission ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form type.
    /// </summary>
    public string FormType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the form was submitted.
    /// </summary>
    public string SubmittedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority level.
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of days waiting.
    /// </summary>
    public int DaysWaiting { get; set; }
}

/// <summary>
/// Handler for GetCoordinatorPendingReviewsQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCoordinatorPendingReviewsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="userManager">The user manager.</param>
public class GetCoordinatorPendingReviewsQueryHandler(
    IBusinessIncubatorRepository repository,
    UserManager<User> userManager) : BaseCommandHandler<GetCoordinatorPendingReviewsQuery, CoordinatorPendingReviewsDto>
{

    /// <inheritdoc/>
    public override async Task<Result<CoordinatorPendingReviewsDto>> Handle(
        GetCoordinatorPendingReviewsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with form submissions
        var project = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(GetCoordinatorPendingReviewsQuery), $"Project with ID {request.ProjectId} not found."));
        }

        // Get submissions pending review
        var pendingSubmissions = project.GetSubmissionsForReview().ToList();
        var now = DateTime.UtcNow;

        var reviews = new List<PendingReviewItem>();

        // Limit to 10 most recent
        foreach (var submission in pendingSubmissions.Take(10))
        {
            var daysWaiting = submission.SubmittedAt.HasValue
                ? (int)(now - submission.SubmittedAt.Value).TotalDays
                : (int)(now - submission.StartedAt).TotalDays;

            // Try to get user name from UserManager if possible
            var user = await userManager.FindByIdAsync(submission.ParticipantUserId);
            var userName = user?.Email?.Split('@')[0] ?? $"Usuario {submission.ParticipantUserId.Substring(0, Math.Min(8, submission.ParticipantUserId.Length))}";

            reviews.Add(new PendingReviewItem
            {
                Id = submission.Id,
                UserId = submission.ParticipantUserId,
                UserName = userName,
                FormType = "Diagnóstico", // For now, all are diagnostic forms
                SubmittedAt = (submission.SubmittedAt ?? submission.StartedAt).ToString("yyyy-MM-dd HH:mm"),
                Status = submission.Status.ToString(),
                Priority = daysWaiting > 7 ? "Alta" : daysWaiting > 3 ? "Media" : "Normal",
                DaysWaiting = daysWaiting,
            });
        }

        // Calculate statistics
        var waitingDays = reviews.Select(r => (double)r.DaysWaiting).ToList();

        var result = new CoordinatorPendingReviewsDto
        {
            TotalPending = pendingSubmissions.Count,
            Reviews = reviews,
            OldestWaitingDays = waitingDays.Any() ? (int)waitingDays.Max() : 0,
            AverageWaitingDays = waitingDays.Any() ? Math.Round(waitingDays.Average(), 1) : 0,
        };

        return Success(result);
    }
}
