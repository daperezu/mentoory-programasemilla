using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.AspNetCore.Identity;
using LinaSys.Auth.Domain.AggregatesModel.User;

namespace LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorRecentActivity;

/// <summary>
/// Query to get recent activity for a coordinator's project.
/// </summary>
[CommandRequiresPermission(PermissionType.ProjectCoordinator)]
public record GetCoordinatorRecentActivityQuery(long ProjectId) : IBaseRequest<List<ActivityItem>>;

/// <summary>
/// Activity item for the dashboard.
/// </summary>
public class ActivityItem
{
    /// <summary>
    /// Gets or sets the activity ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who performed the action.
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action description.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon class.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color theme.
    /// </summary>
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Handler for GetCoordinatorRecentActivityQuery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCoordinatorRecentActivityQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The business incubator repository.</param>
/// <param name="userManager">The user manager.</param>
public class GetCoordinatorRecentActivityQueryHandler(
    IBusinessIncubatorRepository repository,
    UserManager<User> userManager) : BaseCommandHandler<GetCoordinatorRecentActivityQuery, List<ActivityItem>>
{

    /// <inheritdoc/>
    public override async Task<Result<List<ActivityItem>>> Handle(
        GetCoordinatorRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with form submissions and users
        var projectWithSubmissions = await repository.GetProjectWithFormSubmissionsAsync(request.ProjectId, cancellationToken);
        if (projectWithSubmissions is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(GetCoordinatorRecentActivityQuery), $"Project with ID {request.ProjectId} not found."));
        }

        var projectWithUsers = await repository.GetProjectWithUsersAsync(request.ProjectId, cancellationToken);

        var activities = new List<ActivityItem>();
        var activityId = 1;
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        // Add form submission activities
        var recentSubmissions = projectWithSubmissions.FormSubmissions
            .Where(s => s.SubmittedAt.HasValue && s.SubmittedAt.Value >= oneDayAgo)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5);

        foreach (var submission in recentSubmissions)
        {
            var user = await userManager.FindByIdAsync(submission.ParticipantUserId);
            var userName = user?.Email?.Split('@')[0] ?? $"Usuario {submission.ParticipantUserId.Substring(0, Math.Min(8, submission.ParticipantUserId.Length))}";

            activities.Add(new ActivityItem
            {
                Id = activityId++,
                Type = "form_submitted",
                User = userName,
                Action = "Envió formulario de diagnóstico",
                Timestamp = submission.SubmittedAt!.Value.ToString("yyyy-MM-dd HH:mm"),
                Icon = "fas fa-file-alt",
                Color = "primary",
            });
        }

        // Add approved activities
        var recentApprovals = projectWithSubmissions.FormSubmissions
            .Where(s => s.ApprovedAt.HasValue && s.ApprovedAt.Value >= oneDayAgo)
            .OrderByDescending(s => s.ApprovedAt)
            .Take(3);

        foreach (var approval in recentApprovals)
        {
            var user = await userManager.FindByIdAsync(approval.ParticipantUserId);
            var userName = user?.Email?.Split('@')[0] ?? $"Usuario {approval.ParticipantUserId.Substring(0, Math.Min(8, approval.ParticipantUserId.Length))}";

            activities.Add(new ActivityItem
            {
                Id = activityId++,
                Type = "review_completed",
                User = userName,
                Action = "Formulario de diagnóstico aprobado",
                Timestamp = approval.ApprovedAt!.Value.ToString("yyyy-MM-dd HH:mm"),
                Icon = "fas fa-check-circle",
                Color = "info",
            });
        }

        // Add user registration activities
        if (projectWithUsers is not null)
        {
            var recentUsers = projectWithUsers.ProjectUsers
                .Where(u => u.JoinedAt >= oneDayAgo)
                .OrderByDescending(u => u.JoinedAt)
                .Take(3);

            foreach (var projectUser in recentUsers)
            {
                var user = await userManager.FindByIdAsync(projectUser.UserId);
                var userName = user?.Email?.Split('@')[0] ?? $"Usuario {projectUser.UserId.Substring(0, Math.Min(8, projectUser.UserId.Length))}";

                activities.Add(new ActivityItem
                {
                    Id = activityId++,
                    Type = "user_registered",
                    User = userName,
                    Action = $"Se registró como {projectUser.Role}",
                    Timestamp = projectUser.JoinedAt.ToString("yyyy-MM-dd HH:mm"),
                    Icon = "fas fa-user-plus",
                    Color = "success",
                });
            }
        }

        // Sort by timestamp descending and take top 10
        var sortedActivities = activities
            .OrderByDescending(a => DateTime.Parse(a.Timestamp))
            .Take(10)
            .ToList();

        return Success(sortedActivities);
    }
}
