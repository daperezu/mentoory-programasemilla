namespace LinaSys.BusinessIncubator.Infrastructure.Persistence.Entities;

/// <summary>
/// Persistence entity for StarterProgress
/// </summary>
public class StarterProgressEntity
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string CurrentPhase { get; set; } = "diagnosis";
    public DateTime PhaseStartDate { get; set; }
    public DateTime? PhaseExpectedEndDate { get; set; }
    public decimal OverallProgress { get; set; }
    public decimal PhaseProgress { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksTotal { get; set; }
    public int TasksOverdue { get; set; }
    public int FormsCompleted { get; set; }
    public int FormsTotal { get; set; }
    public int FormsRejected { get; set; }
    public int MilestonesAchieved { get; set; }
    public int MilestonesTotal { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? NextMilestoneDate { get; set; }
    public string? NextMilestoneName { get; set; }
    public decimal? EngagementScore { get; set; }
    public decimal? PerformanceScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Domain.Aggregates.BusinessIncubator.Project? Project { get; set; }
}