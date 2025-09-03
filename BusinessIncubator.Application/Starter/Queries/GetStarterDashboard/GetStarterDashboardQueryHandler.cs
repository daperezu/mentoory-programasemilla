using LinaSys.BusinessIncubator.Application.Starter.Mappings;
using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.Auth;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;

public class GetStarterDashboardQueryHandler(
    IBusinessIncubatorRepository repository,
    IStarterRepository starterRepository,
    IActivityTrackingService activityService,
    ICurrentUserService currentUserService,
    ITimeProvider timeProvider) : BaseCommandHandler<GetStarterDashboardQuery, StarterDashboardDto>
{
    public override async Task<Result<StarterDashboardDto>> Handle(GetStarterDashboardQuery request, CancellationToken cancellationToken)
    {
        // Get project information
        var project = await repository.GetProjectByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidOperationException($"Proyecto {request.ProjectId} no encontrado");
        }

        // Get or create starter dashboard
        var currentTime = timeProvider.UtcNow;
        var dashboard = await starterRepository.GetStarterDashboardAsync(request.UserId, request.ProjectId);
        if (dashboard is null)
        {
            dashboard = new StarterDashboard(request.UserId, "starter", request.ProjectId, currentTime);
            await starterRepository.AddDashboardAsync(dashboard);
            await starterRepository.UnitOfWork.SaveEntitiesAsync();
        }

        // Get tasks
        var tasks = await starterRepository.GetStarterTasksAsync(request.UserId, request.ProjectId);

        // Get form submissions
        var formSubmissions = await repository.GetProjectFormSubmissionsByUserAsync(request.ProjectId, request.UserId, cancellationToken);

        // Get recent activities
        var activities = await activityService.GetRecentActivitiesAsync(request.UserId, 10);

        // Get milestones
        var milestones = await starterRepository.GetProjectMilestonesAsync(request.ProjectId);

        // Get mentor info if assigned
        var mentorInfo = await starterRepository.GetMentorInfoAsync(request.ProjectId, request.UserId);

        // Build DTO
        var dto = new StarterDashboardDto
        {
            UserId = request.UserId,
            UserName = currentUserService.UserName ?? "Usuario",
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectDescription = project.Description ?? string.Empty,
            Progress = dashboard.Progress.ToDto(currentTime),
            Tasks = tasks.ToDto(currentTime),
            OverdueTasks = tasks.Where(t => t.IsOverdue(currentTime)).ToDto(currentTime),
            UpcomingTasks = dashboard.GetUpcomingTasks(7, currentTime).ToDto(currentTime),
            MentorInfo = mentorInfo?.ToDto(),
            RecentActivities = activities.ToDto(),
            Milestones = milestones.ToDto(currentTime),
            FormStatuses = MapFormStatuses(formSubmissions),
            Metrics = BuildMetrics(dashboard, tasks.Count, currentTime)
        };

        // Track this access as an activity
        await activityService.TrackActivityAsync(
            request.UserId,
            "dashboard_access",
            "Accedió al dashboard",
            "dashboard",
            dashboard.Id);

        return Success(dto);
    }

    private List<FormStatusDto> MapFormStatuses(IEnumerable<dynamic> formSubmissions)
    {
        var statuses = new List<FormStatusDto>();

        foreach (var submission in formSubmissions)
        {
            statuses.Add(new FormStatusDto
            {
                FormId = submission.Id,
                FormName = submission.FormName ?? "Formulario",
                Status = submission.Status.ToString(),
                SubmittedDate = submission.SubmittedDate,
                ReviewedDate = submission.ReviewedDate,
                ReviewerComments = submission.ReviewerComments,
                ActionUrl = $"/BusinessIncubators/ParticipantForm?projectId={submission.ProjectId}&formId={submission.Id}"
            });
        }

        return statuses;
    }

    private StarterMetricsDto BuildMetrics(StarterDashboard dashboard, int totalTasks, DateTime currentTime)
    {
        var metrics = dashboard.GetMetrics() as StarterMetrics;
        return metrics?.ToDto(dashboard, totalTasks, currentTime) ?? new StarterMetricsDto
        {
            OverallProgress = dashboard.Progress.OverallProgress,
            PendingTasks = 0,
            CompletedTasks = 0,
            TotalTasks = totalTasks,
            UnreadNotifications = 0,
            LastActivityDate = null,
            DaysSinceStart = dashboard.Progress.GetDaysSinceStart(currentTime),
            CurrentPhase = dashboard.Progress.CurrentPhase,
            FormsCompleted = dashboard.Progress.FormsCompleted,
            FormsTotal = dashboard.Progress.FormsTotal,
            OverdueTasks = dashboard.Progress.TasksOverdue,
            CompletionRate = 0,
            ActivityStatus = "Inactivo"
        };
    }
}
