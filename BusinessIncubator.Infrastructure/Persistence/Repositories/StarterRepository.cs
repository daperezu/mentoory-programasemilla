using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.BusinessIncubator.Infrastructure.Persistence.Entities;
using LinaSys.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LinaSys.BusinessIncubator.Infrastructure.Persistence.Repositories;

public class StarterRepository(BusinessIncubatorDbContext context) : AbstractRepository<StarterDashboard>(context), IStarterRepository
{
    private readonly BusinessIncubatorDbContext dbContext = context;
    public async Task<StarterDashboard?> GetStarterDashboardAsync(string userId, long projectId)
    {
        // Query from StarterProgress table
        var progressData = await dbContext.StarterProgress
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);

        if (progressData is not null)
        {
            // Reconstruct dashboard from persisted data
            var dashboard = new StarterDashboard(userId, "starter", projectId, progressData.CreatedAt);

            // Update progress values
            dashboard.UpdateProgress(
                progressData.OverallProgress,
                progressData.CurrentPhase,
                progressData.TasksCompleted,
                progressData.TasksTotal,
                progressData.FormsCompleted,
                progressData.FormsTotal,
                progressData.LastActivityDate ?? progressData.CreatedAt);

            // Load tasks
            var tasks = await GetTasksAsync(userId, projectId);
            foreach (var task in tasks)
            {
                dashboard.AddTask(task);
            }

            // TODO: Load notifications from Core.Notifications when available
            return dashboard;
        }

        return null;
    }

    public async Task<List<StarterTask>> GetTasksAsync(string userId, long projectId, int count = 10)
    {
        var taskEntities = await dbContext.StarterTasks
            .Where(t => t.UserId == userId && t.ProjectId == projectId)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Take(count)
            .ToListAsync();

        var tasks = new List<StarterTask>();
        foreach (var entity in taskEntities)
        {
            var task = new StarterTask(
                entity.ProjectId,
                entity.UserId,
                entity.Title,
                entity.Description ?? string.Empty,
                entity.CreatedAt,
                entity.Type,
                entity.Priority.ToString(),
                entity.DueDate,
                entity.CreatedBy,
                entity.Category);

            // Update task status if needed
            if (entity.Status == "completed" && entity.CompletedAt.HasValue)
            {
                task.MarkAsCompleted(entity.CompletedAt.Value, entity.CancellationReason);
            }

            tasks.Add(task);
        }

        return tasks;
    }

    public async Task<int> GetUserActivityCountAsync(string userId, DateTime? since = null)
    {
        var query = dbContext.StarterTasks.Where(t => t.UserId == userId && t.Status == "completed");

        if (since.HasValue)
        {
            query = query.Where(t => t.CompletedAt >= since.Value);
        }

        return await query.CountAsync();
    }

    public async Task<List<ProjectMilestone>> GetMilestonesAsync(long projectId)
    {
        // TODO: Implement when ProjectMilestones table is available
        await Task.CompletedTask;
        return [];
    }

    public async Task<decimal> CalculateProgressAsync(string userId, long projectId)
    {
        var progress = await dbContext.StarterProgress
            .Where(sp => sp.UserId == userId && sp.ProjectId == projectId)
            .Select(sp => sp.OverallProgress)
            .FirstOrDefaultAsync();

        return progress;
    }

    public async Task UpdateProgressAsync(string userId, long projectId, decimal overallProgress, string currentPhase)
    {
        var progress = await dbContext.StarterProgress
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);

        if (progress != null)
        {
            progress.OverallProgress = overallProgress;
            progress.CurrentPhase = currentPhase;
            progress.UpdatedAt = DateTime.UtcNow;
            progress.LastActivityDate = DateTime.UtcNow;

            // SaveChangesAsync should be called via UnitOfWork
        }
        else
        {
            // Create new progress record
            var newProgress = new StarterProgressEntity
            {
                UserId = userId,
                ProjectId = projectId,
                CurrentPhase = currentPhase,
                PhaseStartDate = DateTime.UtcNow,
                OverallProgress = overallProgress,
                PhaseProgress = 0,
                TasksCompleted = 0,
                TasksTotal = 0,
                TasksOverdue = 0,
                FormsCompleted = 0,
                FormsTotal = 0,
                FormsRejected = 0,
                MilestonesAchieved = 0,
                MilestonesTotal = 0,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.StarterProgress.Add(newProgress);
            // SaveChangesAsync should be called via UnitOfWork
        }
    }

    public async Task<List<StarterMetrics>> GetMetricsHistoryAsync(string userId, long projectId, int days = 30)
    {
        // TODO: Implement metrics history tracking
        await Task.CompletedTask;
        return [];
    }

    public async Task<List<StarterResource>> GetResourcesAsync(long projectId, string? phase = null)
    {
        // TODO: Implement when StarterResources table is available
        await Task.CompletedTask;
        return [];
    }

    public async Task<MentorInfo?> GetAssignedMentorAsync(long projectId)
    {
        // TODO: Implement when mentor assignment data is available
        await Task.CompletedTask;
        return null;
    }

    public async Task UpdateTaskStatusAsync(long taskId, string status, string? completedBy = null)
    {
        var task = await dbContext.StarterTasks.FindAsync(taskId);

        if (task != null)
        {
            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;

            if (status == "completed")
            {
                task.CompletedAt = DateTime.UtcNow;
                task.CompletedBy = completedBy;
            }
            else if (status == "in_progress")
            {
                task.StartedAt = DateTime.UtcNow;
            }

            // SaveChangesAsync should be called via UnitOfWork
        }
    }

    public async Task<int> GetOverdueTasksCountAsync(string userId, long projectId)
    {
        var now = DateTime.UtcNow;
        return await dbContext.StarterTasks
            .CountAsync(t => t.UserId == userId
                && t.ProjectId == projectId
                && t.Status != "completed"
                && t.Status != "cancelled"
                && t.DueDate.HasValue
                && t.DueDate < now);
    }

    public async Task<Dictionary<string, int>> GetTasksGroupedByStatusAsync(string userId, long projectId)
    {
        var result = await dbContext.StarterTasks
            .Where(t => t.UserId == userId && t.ProjectId == projectId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return result;
    }

    public async Task<bool> HasActiveProgressAsync(string userId, long projectId)
    {
        return await dbContext.StarterProgress
            .AnyAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);
    }

    public async Task CreateTaskAsync(StarterTask task, string userId, long projectId)
    {
        var taskEntity = new StarterTaskEntity
        {
            UserId = userId,
            ProjectId = projectId,
            Title = task.Title,
            Description = task.Description,
            Type = task.Type,
            Priority = int.Parse(task.Priority),
            Status = task.Status.ToString().ToLower(),
            DueDate = task.DueDate,
            Category = task.Category,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        dbContext.StarterTasks.Add(taskEntity);
        // SaveChangesAsync should be called via UnitOfWork
        await Task.CompletedTask; // Keep method async for interface compatibility
    }

    public async Task<int> GetCompletedFormsCountAsync(string userId, long projectId)
    {
        var progress = await dbContext.StarterProgress
            .Where(sp => sp.UserId == userId && sp.ProjectId == projectId)
            .Select(sp => sp.FormsCompleted)
            .FirstOrDefaultAsync();

        return progress;
    }

    public async Task<DateTime?> GetLastActivityDateAsync(string userId, long projectId)
    {
        var progress = await dbContext.StarterProgress
            .Where(sp => sp.UserId == userId && sp.ProjectId == projectId)
            .Select(sp => sp.LastActivityDate)
            .FirstOrDefaultAsync();

        return progress;
    }

    public async Task<Dictionary<string, decimal>> GetPhaseProgressAsync(string userId, long projectId)
    {
        var progress = await dbContext.StarterProgress
            .Where(sp => sp.UserId == userId && sp.ProjectId == projectId)
            .Select(sp => new { sp.CurrentPhase, sp.PhaseProgress })
            .FirstOrDefaultAsync();

        if (progress != null)
        {
            return new Dictionary<string, decimal>
            {
                { progress.CurrentPhase, progress.PhaseProgress }
            };
        }

        return [];
    }

    public async Task AddDashboardAsync(StarterDashboard dashboard)
    {
        // StarterDashboard is not persisted as a single entity
        // It's composed of progress, tasks, etc.
        // So we don't need to implement this
        await Task.CompletedTask;
    }

    public async Task UpdateDashboardAsync(StarterDashboard dashboard)
    {
        // StarterDashboard is not persisted as a single entity
        // It's composed of progress, tasks, etc.
        // So we don't need to implement this
        await Task.CompletedTask;
    }

    public async Task<List<StarterTask>> GetStarterTasksAsync(string userId, long projectId)
    {
        return await GetTasksAsync(userId, projectId);
    }

    public async Task<List<StarterTask>> GetTasksAsync(string userId, long projectId)
    {
        return await GetTasksAsync(userId, projectId, 100);
    }

    public async Task<StarterTask?> GetTaskByIdAsync(long taskId)
    {
        var entity = await dbContext.StarterTasks.FindAsync(taskId);

        if (entity == null)
        {
            return null;
        }

        var task = new StarterTask(
            entity.ProjectId,
            entity.UserId,
            entity.Title,
            entity.Description ?? string.Empty,
            entity.CreatedAt,
            entity.Type,
            entity.Priority.ToString(),
            entity.DueDate,
            entity.CreatedBy,
            entity.Category);

        if (entity.Status == "completed" && entity.CompletedAt.HasValue)
        {
            task.MarkAsCompleted(entity.CompletedAt.Value, entity.CancellationReason);
        }

        return task;
    }

    public async Task AddTaskAsync(StarterTask task)
    {
        var taskEntity = new StarterTaskEntity
        {
            UserId = task.UserId,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Type = task.Type,
            Priority = int.Parse(task.Priority),
            Status = task.Status.ToString().ToLower(),
            DueDate = task.DueDate,
            Category = task.Category,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = task.AssignedBy ?? task.UserId
        };

        dbContext.StarterTasks.Add(taskEntity);
        // SaveChangesAsync should be called via UnitOfWork
        await Task.CompletedTask; // Keep method async for interface compatibility
    }

    public async Task UpdateTaskAsync(StarterTask task)
    {
        // Need to find the task entity by some identifier
        // For now, this is a placeholder implementation
        await Task.CompletedTask;
    }

    public async Task<List<ProjectMilestone>> GetProjectMilestonesAsync(long projectId)
    {
        return await GetMilestonesAsync(projectId);
    }

    public async Task<MentorInfo?> GetMentorInfoAsync(long projectId, string userId)
    {
        return await GetAssignedMentorAsync(projectId);
    }

    public async Task<StarterResource?> GetResourceByIdAsync(long resourceId)
    {
        // TODO: Implement when StarterResources table is available
        await Task.CompletedTask;
        return null;
    }

    public async Task RecordResourceViewAsync(long resourceId, string userId)
    {
        // TODO: Implement when resource view tracking is available
        await Task.CompletedTask;
    }

    public async Task MarkNotificationAsReadAsync(long notificationId)
    {
        // TODO: Implement when notifications table is available
        // For now, this is a placeholder implementation
        await Task.CompletedTask;
    }
}
