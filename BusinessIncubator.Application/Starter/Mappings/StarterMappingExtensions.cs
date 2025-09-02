using LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;
using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.Core.Application.Dashboard.Services;

namespace LinaSys.BusinessIncubator.Application.Starter.Mappings;

public static class StarterMappingExtensions
{
    public static StarterProgressDto ToDto(this StarterProgress progress, DateTime currentTime)
    {
        return new StarterProgressDto
        {
            CurrentPhase = progress.CurrentPhase,
            PhaseStartDate = progress.PhaseStartDate,
            PhaseExpectedEndDate = progress.PhaseExpectedEndDate,
            OverallProgress = progress.OverallProgress,
            PhaseProgress = progress.PhaseProgress,
            TasksCompleted = progress.TasksCompleted,
            TasksTotal = progress.TasksTotal,
            TasksOverdue = progress.TasksOverdue,
            FormsCompleted = progress.FormsCompleted,
            FormsTotal = progress.FormsTotal,
            FormsRejected = progress.FormsRejected,
            MilestonesAchieved = progress.MilestonesAchieved,
            MilestonesTotal = progress.MilestonesTotal,
            LastActivityDate = progress.LastActivityDate,
            NextMilestoneDate = progress.NextMilestoneDate,
            NextMilestoneName = progress.NextMilestoneName,
            EngagementScore = progress.EngagementScore,
            PerformanceScore = progress.PerformanceScore,
            DaysSinceStart = progress.GetDaysSinceStart(currentTime)
        };
    }

    public static StarterTaskDto ToDto(this StarterTask task, DateTime currentTime)
    {
        return new StarterTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Type = string.Empty, // Set as needed
            Status = task.Status.ToString(),
            Priority = task.Priority,
            DueDate = task.DueDate,
            CompletedDate = task.CompletedDate,
            AssignedBy = task.AssignedBy,
            Category = task.Category,
            IsOverdue = task.IsOverdue(currentTime),
            DaysUntilDue = task.GetDaysUntilDue(currentTime),
            ActionUrl = null // Set as needed
        };
    }

    public static MentorInfoDto ToDto(this MentorInfo mentorInfo)
    {
        return new MentorInfoDto
        {
            MentorId = mentorInfo.MentorId,
            MentorName = mentorInfo.MentorName,
            MentorEmail = mentorInfo.MentorEmail,
            MentorPhone = mentorInfo.MentorPhone,
            MentorPhoto = mentorInfo.MentorPhoto,
            NextMeetingDate = mentorInfo.NextMeetingDate,
            MeetingUrl = null, // Set as needed
            SessionsCompleted = 0, // Set as needed
            SessionsTotal = 0 // Set as needed
        };
    }

    public static RecentActivityDto ToDto(this UserActivityDto activity)
    {
        return new RecentActivityDto
        {
            Id = activity.Id,
            Type = activity.ActivityType,
            Description = activity.Description,
            Date = activity.CreatedDate,
            Icon = GetActivityIcon(activity.ActivityType),
            ActionUrl = null // Set as needed
        };
    }

    public static MilestoneDto ToDto(this ProjectMilestone milestone, DateTime currentTime)
    {
        return new MilestoneDto
        {
            Id = milestone.Id,
            Name = milestone.Name,
            Description = milestone.Description,
            TargetDate = currentTime, // Set appropriately
            CompletedDate = milestone.CompletedDate,
            Status = milestone.Status,
            Progress = milestone.Progress,
            Requirements = milestone.GetRequirementsList()
        };
    }

    public static StarterMetricsDto ToDto(this StarterMetrics metrics, StarterDashboard dashboard, int totalTasks, DateTime currentTime)
    {
        return new StarterMetricsDto
        {
            OverallProgress = dashboard.Progress.OverallProgress,
            PendingTasks = metrics.PendingItems,
            CompletedTasks = metrics.CompletedItems,
            TotalTasks = totalTasks,
            UnreadNotifications = metrics.UnreadNotifications,
            LastActivityDate = metrics.LastActivityDate,
            DaysSinceStart = dashboard.Progress.GetDaysSinceStart(currentTime),
            CurrentPhase = dashboard.Progress.CurrentPhase,
            FormsCompleted = dashboard.Progress.FormsCompleted,
            FormsTotal = dashboard.Progress.FormsTotal,
            OverdueTasks = dashboard.Progress.TasksOverdue,
            CompletionRate = metrics.GetCompletionRate(),
            ActivityStatus = metrics.GetActivityStatus()
        };
    }

    public static List<StarterTaskDto> ToDto(this IEnumerable<StarterTask> tasks, DateTime currentTime)
    {
        return tasks.Select(t => t.ToDto(currentTime)).ToList();
    }

    public static List<RecentActivityDto> ToDto(this IEnumerable<UserActivityDto> activities)
    {
        return activities.Select(a => a.ToDto()).ToList();
    }

    public static List<MilestoneDto> ToDto(this IEnumerable<ProjectMilestone> milestones, DateTime currentTime)
    {
        return milestones.Select(m => m.ToDto(currentTime)).ToList();
    }

    private static string GetActivityIcon(string activityType)
    {
        return activityType switch
        {
            "login" => "fa-sign-in-alt",
            "form_submission" => "fa-file-alt",
            "task_completed" => "fa-check-circle",
            "document_uploaded" => "fa-upload",
            "meeting_scheduled" => "fa-calendar",
            _ => "fa-circle"
        };
    }
}