using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Repositories;

public interface IStarterRepository : IRepository<StarterDashboard>
{
    Task<StarterDashboard?> GetStarterDashboardAsync(string userId, long projectId);
    Task AddDashboardAsync(StarterDashboard dashboard);
    Task UpdateDashboardAsync(StarterDashboard dashboard);
    Task<List<StarterTask>> GetStarterTasksAsync(string userId, long projectId);
    Task<List<StarterTask>> GetTasksAsync(string userId, long projectId);
    Task<StarterTask?> GetTaskByIdAsync(long taskId);
    Task AddTaskAsync(StarterTask task);
    Task UpdateTaskAsync(StarterTask task);
    Task<List<ProjectMilestone>> GetProjectMilestonesAsync(long projectId);
    Task<MentorInfo?> GetMentorInfoAsync(long projectId, string userId);
    Task<List<StarterResource>> GetResourcesAsync(long projectId, string? phase = null);
    Task<StarterResource?> GetResourceByIdAsync(long resourceId);
    Task RecordResourceViewAsync(long resourceId, string userId);

    // Additional methods for progress calculation
    Task<decimal> CalculateProgressAsync(string userId, long projectId);
    Task UpdateProgressAsync(string userId, long projectId, decimal overallProgress, string currentPhase);
    Task<Dictionary<string, int>> GetTasksGroupedByStatusAsync(string userId, long projectId);
    Task<int> GetOverdueTasksCountAsync(string userId, long projectId);
    Task<int> GetCompletedFormsCountAsync(string userId, long projectId);
    Task<DateTime?> GetLastActivityDateAsync(string userId, long projectId);
    Task UpdateTaskStatusAsync(long taskId, string status, string? completedBy = null);
    Task MarkNotificationAsReadAsync(long notificationId);
}