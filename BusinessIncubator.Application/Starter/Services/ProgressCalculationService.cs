using LinaSys.BusinessIncubator.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public class ProgressCalculationService(
    IStarterRepository starterRepository,
    ILogger<ProgressCalculationService> logger) : IProgressCalculationService
{
    // Weights for different progress components
    private const decimal TaskWeight = 0.40m;
    private const decimal FormWeight = 0.35m;
    private const decimal MilestoneWeight = 0.25m;

    // Phase order and weights
    private readonly Dictionary<string, int> _phaseOrder = new()
    {
        ["diagnosis"] = 1,
        ["development"] = 2,
        ["validation"] = 3,
        ["implementation"] = 4,
        ["growth"] = 5
    };

    private readonly Dictionary<string, decimal> _phaseWeights = new()
    {
        ["diagnosis"] = 0.15m,
        ["development"] = 0.25m,
        ["validation"] = 0.20m,
        ["implementation"] = 0.25m,
        ["growth"] = 0.15m
    };

    public async Task<decimal> CalculateOverallProgressAsync(string userId, long projectId)
    {
        // Get current progress from repository
        var currentProgress = await starterRepository.CalculateProgressAsync(userId, projectId);

        // Get task completion stats
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var totalTasks = taskStats.Sum(kvp => kvp.Value);
        var completedTasks = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0;

        decimal taskProgress = totalTasks > 0 ? (decimal)completedTasks / totalTasks : 0;

        // Get forms completed
        var formsCompleted = await starterRepository.GetCompletedFormsCountAsync(userId, projectId);
        decimal formProgress = formsCompleted > 0 ? Math.Min(formsCompleted / 5m, 1m) : 0; // Assume 5 forms total

        // Calculate weighted progress
        decimal overallProgress = (taskProgress * TaskWeight) + (formProgress * FormWeight) + (currentProgress * MilestoneWeight);
        overallProgress = Math.Min(Math.Round(overallProgress * 100, 2), 100);

        logger.LogInformation("Calculated overall progress for user {UserId} project {ProjectId}: {Progress}%",
            userId, projectId, overallProgress);

        return overallProgress;
    }

    public async Task<string> DetermineCurrentPhaseAsync(string userId, long projectId)
    {
        // Get current progress percentage
        var progress = await starterRepository.CalculateProgressAsync(userId, projectId);

        // Determine phase based on progress
        string phase = progress switch
        {
            < 20 => "diagnosis",
            < 40 => "development",
            < 60 => "validation",
            < 80 => "implementation",
            _ => "growth"
        };

        logger.LogDebug("Determined phase {Phase} for user {UserId} project {ProjectId}",
            phase, userId, projectId);

        return phase;
    }

    public async Task<decimal> CalculatePhaseProgressAsync(string userId, long projectId, string phase)
    {
        // Get task stats for the current phase
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var totalTasks = taskStats.Sum(kvp => kvp.Value);
        var completedTasks = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0;

        decimal phaseProgress = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;
        phaseProgress = Math.Min(Math.Round(phaseProgress, 2), 100);

        logger.LogDebug("Calculated phase progress for {Phase}: {Progress}%", phase, phaseProgress);

        return phaseProgress;
    }

    public async Task<Dictionary<string, decimal>> GetPhaseProgressDetailsAsync(string userId, long projectId, string phase)
    {
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var totalTasks = taskStats.Sum(kvp => kvp.Value);
        var completedTasks = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0;

        var formsCompleted = await starterRepository.GetCompletedFormsCountAsync(userId, projectId);

        return new Dictionary<string, decimal>
        {
            ["tasks"] = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0,
            ["forms"] = formsCompleted > 0 ? Math.Min(formsCompleted / 5m * 100, 100) : 0,
            ["milestones"] = 0, // TODO: Implement milestone tracking
            ["overall"] = await CalculatePhaseProgressAsync(userId, projectId, phase)
        };
    }

    public async Task UpdateProgressAsync(string userId, long projectId)
    {
        var overallProgress = await CalculateOverallProgressAsync(userId, projectId);
        var currentPhase = await DetermineCurrentPhaseAsync(userId, projectId);

        await starterRepository.UpdateProgressAsync(userId, projectId, overallProgress, currentPhase);

        logger.LogInformation("Updated progress for user {UserId} project {ProjectId}: {Progress}% in phase {Phase}",
            userId, projectId, overallProgress, currentPhase);
    }

    public async Task<ProgressReport> GenerateProgressReportAsync(string userId, long projectId)
    {
        var dashboard = await starterRepository.GetStarterDashboardAsync(userId, projectId);
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var overallProgress = await CalculateOverallProgressAsync(userId, projectId);
        var currentPhase = await DetermineCurrentPhaseAsync(userId, projectId);

        return new ProgressReport
        {
            UserId = userId,
            ProjectId = projectId,
            OverallProgress = overallProgress,
            CurrentPhase = currentPhase,
            TasksCompleted = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0,
            TasksTotal = taskStats.Sum(kvp => kvp.Value),
            FormsCompleted = await starterRepository.GetCompletedFormsCountAsync(userId, projectId),
            FormsTotal = 5, // TODO: Get actual form count
            MilestonesAchieved = 0, // TODO: Implement milestone tracking
            MilestonesTotal = 0,
            LastActivityDate = await starterRepository.GetLastActivityDateAsync(userId, projectId),
            GeneratedDate = DateTime.UtcNow
        };
    }

    public async Task<List<WeeklyProgress>> GetWeeklyProgressAsync(string userId, long projectId, int weeks = 8)
    {
        // TODO: Implement weekly progress tracking
        // For now, return empty list
        logger.LogDebug("Getting weekly progress for user {UserId} project {ProjectId}", userId, projectId);
        await Task.CompletedTask;
        return [];
    }

    public async Task<bool> IsPhaseCompleteAsync(string userId, long projectId, string phase)
    {
        var phaseProgress = await CalculatePhaseProgressAsync(userId, projectId, phase);
        return phaseProgress >= 100;
    }

    public async Task<string?> GetNextPhaseAsync(string currentPhase)
    {
        await Task.CompletedTask;

        if (!_phaseOrder.ContainsKey(currentPhase))
        {
            return null;
        }

        var currentOrder = _phaseOrder[currentPhase];
        var nextPhase = _phaseOrder.FirstOrDefault(p => p.Value == currentOrder + 1);

        return nextPhase.Key;
    }

    public async Task<decimal> CalculateProjectedCompletionPercentageAsync(string userId, long projectId)
    {
        var weeklyProgress = await GetWeeklyProgressAsync(userId, projectId, 4);

        if (!weeklyProgress.Any())
        {
            return await CalculateOverallProgressAsync(userId, projectId);
        }

        // Simple linear projection based on average weekly progress
        var avgWeeklyProgress = weeklyProgress.Average(w => w.ProgressDelta);
        var currentProgress = await CalculateOverallProgressAsync(userId, projectId);

        // Project 4 weeks ahead
        var projectedProgress = currentProgress + (avgWeeklyProgress * 4);

        return Math.Min(projectedProgress, 100);
    }

    public async Task<ProgressMetrics> CalculateDetailedProgressAsync(string userId, long projectId)
    {
        var overallProgress = await CalculateOverallProgressAsync(userId, projectId);
        var currentPhase = await DetermineCurrentPhaseAsync(userId, projectId);
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var overdueCount = await starterRepository.GetOverdueTasksCountAsync(userId, projectId);
        var formsCompleted = await starterRepository.GetCompletedFormsCountAsync(userId, projectId);

        var phaseProgressDict = new Dictionary<string, decimal>();
        foreach (var phase in _phaseOrder.Keys)
        {
            phaseProgressDict[phase] = await CalculatePhaseProgressAsync(userId, projectId, phase);
        }

        return new ProgressMetrics
        {
            OverallProgress = overallProgress,
            CurrentPhase = currentPhase,
            TasksCompleted = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0,
            TasksTotal = taskStats.Sum(kvp => kvp.Value),
            TasksOverdue = overdueCount,
            FormsCompleted = formsCompleted,
            FormsTotal = 5, // TODO: Get actual form count
            MilestonesCompleted = 0, // TODO: Implement milestone tracking
            MilestonesTotal = 0,
            PhaseProgress = phaseProgressDict,
            EstimatedCompletionDate = null, // TODO: Calculate based on velocity
            VelocityRate = 0 // TODO: Calculate based on historical data
        };
    }

    public async Task<PhaseProgress> GetPhaseProgressAsync(string userId, long projectId, string phase)
    {
        var taskStats = await starterRepository.GetTasksGroupedByStatusAsync(userId, projectId);
        var formsCompleted = await starterRepository.GetCompletedFormsCountAsync(userId, projectId);
        var phaseProgressValue = await CalculatePhaseProgressAsync(userId, projectId, phase);
        var currentPhase = await DetermineCurrentPhaseAsync(userId, projectId);

        return new PhaseProgress
        {
            Phase = phase,
            Progress = phaseProgressValue,
            TasksCompleted = taskStats.ContainsKey("completed") ? taskStats["completed"] : 0,
            TasksTotal = taskStats.Sum(kvp => kvp.Value),
            FormsCompleted = formsCompleted,
            FormsTotal = 5, // TODO: Get actual form count
            StartDate = null, // TODO: Track phase start dates
            CompletedDate = phaseProgressValue >= 100 ? DateTime.UtcNow : null,
            IsActive = phase == currentPhase,
            IsCompleted = phaseProgressValue >= 100
        };
    }
}

public class ProgressReport
{
    public string UserId { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public decimal OverallProgress { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int TasksTotal { get; set; }
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public int MilestonesAchieved { get; set; }
    public int MilestonesTotal { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime GeneratedDate { get; set; }
}

public class WeeklyProgress
{
    public int Week { get; set; }
    public decimal Progress { get; set; }
    public decimal ProgressDelta { get; set; }
    public int TasksCompleted { get; set; }
    public DateTime WeekStartDate { get; set; }
}
