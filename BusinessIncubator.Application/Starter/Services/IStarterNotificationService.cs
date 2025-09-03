using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public interface IStarterNotificationService
{
    Task SendTaskReminderAsync(string userId, long taskId);
    Task SendMilestoneCompletedAsync(string userId, long projectId, string milestoneName);
    Task SendPhaseCompletedAsync(string userId, long projectId, string phase);
    Task SendMentorAssignedAsync(string userId, long projectId, string mentorName);
    Task SendMeetingReminderAsync(string userId, long projectId, DateTime meetingDate);
    Task SendProgressUpdateAsync(string userId, long projectId, decimal progress);
    Task SendOverdueTasksAlertAsync(string userId, long projectId, int overdueCount);
    Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int count = 10);
    Task MarkNotificationAsReadAsync(long notificationId);
    Task DismissNotificationAsync(long notificationId);
}

public class UserNotification
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // info, warning, success, error
    public string Category { get; set; } = string.Empty; // task, milestone, meeting, progress
    public string Priority { get; set; } = string.Empty; // low, normal, high, urgent
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime? DismissedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}