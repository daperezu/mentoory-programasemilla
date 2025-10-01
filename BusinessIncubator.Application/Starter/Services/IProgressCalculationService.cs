namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public interface IProgressCalculationService
{
    Task<decimal> CalculateOverallProgressAsync(string userId, long projectId);
    Task<ProgressMetrics> CalculateDetailedProgressAsync(string userId, long projectId);
    Task<string> DetermineCurrentPhaseAsync(string userId, long projectId);
    Task<PhaseProgress> GetPhaseProgressAsync(string userId, long projectId, string phase);
    Task UpdateProgressAsync(string userId, long projectId);
}

public class ProgressMetrics
{
    public decimal OverallProgress { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int TasksTotal { get; set; }
    public int TasksOverdue { get; set; }
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public int MilestonesCompleted { get; set; }
    public int MilestonesTotal { get; set; }
    public Dictionary<string, decimal> PhaseProgress { get; set; } = [];
    public DateTime? EstimatedCompletionDate { get; set; }
    public decimal VelocityRate { get; set; }
}

public class PhaseProgress
{
    public string Phase { get; set; } = string.Empty;
    public decimal Progress { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksTotal { get; set; }
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
}
