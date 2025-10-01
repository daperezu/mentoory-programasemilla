using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

/// <summary>
/// Starter progress value object
/// </summary>
public class StarterProgress() : ValueObject
{
    public StarterProgress(
        decimal overallProgress,
        string currentPhase,
        int tasksCompleted,
        int tasksTotal,
        int tasksOverdue,
        int formsCompleted,
        int formsTotal,
        int milestonesCompleted,
        int milestonesTotal,
        DateTime startDate,
        DateTime? lastActivityDate,
        int daysSinceStart)
        : this()
    {
        OverallProgress = ValidatePercentage(overallProgress, nameof(overallProgress));
        CurrentPhase = currentPhase ?? "diagnosis";
        TasksCompleted = ValidateNonNegative(tasksCompleted, nameof(tasksCompleted));
        TasksTotal = ValidateNonNegative(tasksTotal, nameof(tasksTotal));
        TasksOverdue = ValidateNonNegative(tasksOverdue, nameof(tasksOverdue));
        FormsCompleted = ValidateNonNegative(formsCompleted, nameof(formsCompleted));
        FormsTotal = ValidateNonNegative(formsTotal, nameof(formsTotal));
        MilestonesAchieved = ValidateNonNegative(milestonesCompleted, nameof(milestonesCompleted));
        MilestonesTotal = ValidateNonNegative(milestonesTotal, nameof(milestonesTotal));
        PhaseStartDate = startDate;
        LastActivityDate = lastActivityDate;
    }

    public StarterProgress(
        string currentPhase,
        DateTime phaseStartDate,
        DateTime? phaseExpectedEndDate,
        decimal overallProgress,
        decimal phaseProgress,
        int tasksCompleted,
        int tasksTotal,
        int tasksOverdue,
        int formsCompleted,
        int formsTotal,
        int formsRejected,
        int milestonesAchieved,
        int milestonesTotal,
        DateTime? lastActivityDate,
        DateTime? nextMilestoneDate,
        string? nextMilestoneName,
        decimal? engagementScore,
        decimal? performanceScore)
        : this()
    {
        CurrentPhase = currentPhase ?? throw new ArgumentNullException(nameof(currentPhase));
        PhaseStartDate = phaseStartDate;
        PhaseExpectedEndDate = phaseExpectedEndDate;
        OverallProgress = ValidatePercentage(overallProgress, nameof(overallProgress));
        PhaseProgress = ValidatePercentage(phaseProgress, nameof(phaseProgress));
        TasksCompleted = ValidateNonNegative(tasksCompleted, nameof(tasksCompleted));
        TasksTotal = ValidateNonNegative(tasksTotal, nameof(tasksTotal));
        TasksOverdue = ValidateNonNegative(tasksOverdue, nameof(tasksOverdue));
        FormsCompleted = ValidateNonNegative(formsCompleted, nameof(formsCompleted));
        FormsTotal = ValidateNonNegative(formsTotal, nameof(formsTotal));
        FormsRejected = ValidateNonNegative(formsRejected, nameof(formsRejected));
        MilestonesAchieved = ValidateNonNegative(milestonesAchieved, nameof(milestonesAchieved));
        MilestonesTotal = ValidateNonNegative(milestonesTotal, nameof(milestonesTotal));
        LastActivityDate = lastActivityDate;
        NextMilestoneDate = nextMilestoneDate;
        NextMilestoneName = nextMilestoneName;
        EngagementScore = engagementScore.HasValue ? ValidatePercentage(engagementScore.Value, nameof(engagementScore)) : null;
        PerformanceScore = performanceScore.HasValue ? ValidatePercentage(performanceScore.Value, nameof(performanceScore)) : null;
    }

    /// <summary>
    /// Gets current phase
    /// </summary>
    public string CurrentPhase { get; private set; } = "diagnosis";

    /// <summary>
    /// Gets phase start date
    /// </summary>
    public DateTime PhaseStartDate { get; private set; }

    /// <summary>
    /// Gets phase expected end date
    /// </summary>
    public DateTime? PhaseExpectedEndDate { get; private set; }

    /// <summary>
    /// Gets overall progress percentage
    /// </summary>
    public decimal OverallProgress { get; private set; } = 0;

    /// <summary>
    /// Gets phase progress percentage
    /// </summary>
    public decimal PhaseProgress { get; private set; } = 0;

    /// <summary>
    /// Gets tasks completed
    /// </summary>
    public int TasksCompleted { get; private set; } = 0;

    /// <summary>
    /// Gets total tasks
    /// </summary>
    public int TasksTotal { get; private set; } = 0;

    /// <summary>
    /// Gets tasks overdue
    /// </summary>
    public int TasksOverdue { get; private set; } = 0;

    /// <summary>
    /// Gets forms completed
    /// </summary>
    public int FormsCompleted { get; private set; } = 0;

    /// <summary>
    /// Gets total forms
    /// </summary>
    public int FormsTotal { get; private set; } = 0;

    /// <summary>
    /// Gets forms rejected
    /// </summary>
    public int FormsRejected { get; private set; } = 0;

    /// <summary>
    /// Gets milestones achieved
    /// </summary>
    public int MilestonesAchieved { get; private set; } = 0;

    /// <summary>
    /// Gets total milestones
    /// </summary>
    public int MilestonesTotal { get; private set; } = 0;

    /// <summary>
    /// Gets last activity date
    /// </summary>
    public DateTime? LastActivityDate { get; private set; }

    /// <summary>
    /// Gets next milestone date
    /// </summary>
    public DateTime? NextMilestoneDate { get; private set; }

    /// <summary>
    /// Gets next milestone name
    /// </summary>
    public string? NextMilestoneName { get; private set; }

    /// <summary>
    /// Gets engagement score
    /// </summary>
    public decimal? EngagementScore { get; private set; }

    /// <summary>
    /// Gets performance score
    /// </summary>
    public decimal? PerformanceScore { get; private set; }

    /// <summary>
    /// Gets start date - used for initial startup tracking
    /// </summary>
    public DateTime StartDate => PhaseStartDate;

    /// <summary>
    /// Gets milestones completed (alias for MilestonesAchieved)
    /// </summary>
    public int MilestonesCompleted => MilestonesAchieved;

    /// <summary>
    /// Days since start
    /// </summary>
    /// <param name="currentDate">The current date to calculate from.</param>
    /// <returns></returns>
    public int GetDaysSinceStart(DateTime currentDate) => (currentDate - PhaseStartDate).Days;

    /// <summary>
    /// Create progress with updated task progress
    /// </summary>
    /// <param name="completed">Number of completed tasks.</param>
    /// <param name="total">Total number of tasks.</param>
    /// <param name="overallProgress">Overall progress percentage.</param>
    /// <param name="activityDate">The activity date.</param>
    /// <returns></returns>
    public StarterProgress WithTaskProgress(int completed, int total, decimal overallProgress, DateTime activityDate)
    {
        return new StarterProgress(
            CurrentPhase,
            PhaseStartDate,
            PhaseExpectedEndDate,
            overallProgress,
            PhaseProgress,
            completed,
            total,
            TasksOverdue,
            FormsCompleted,
            FormsTotal,
            FormsRejected,
            MilestonesAchieved,
            MilestonesTotal,
            activityDate,
            NextMilestoneDate,
            NextMilestoneName,
            EngagementScore,
            PerformanceScore);
    }

    /// <summary>
    /// Create progress with updated form progress
    /// </summary>
    /// <param name="completed">Number of completed forms.</param>
    /// <param name="total">Total number of forms.</param>
    /// <param name="rejected">Number of rejected forms.</param>
    /// <param name="activityDate">The activity date.</param>
    /// <returns></returns>
    public StarterProgress WithFormProgress(int completed, int total, int rejected, DateTime activityDate)
    {
        return new StarterProgress(
            CurrentPhase,
            PhaseStartDate,
            PhaseExpectedEndDate,
            OverallProgress,
            PhaseProgress,
            TasksCompleted,
            TasksTotal,
            TasksOverdue,
            completed,
            total,
            rejected,
            MilestonesAchieved,
            MilestonesTotal,
            activityDate,
            NextMilestoneDate,
            NextMilestoneName,
            EngagementScore,
            PerformanceScore);
    }

    /// <summary>
    /// Create progress with phase update
    /// </summary>
    /// <param name="phase">The new phase.</param>
    /// <param name="phaseProgress">The phase progress percentage.</param>
    /// <param name="phaseStartDate">The phase start date.</param>
    /// <param name="activityDate">The activity date.</param>
    /// <returns></returns>
    public StarterProgress WithPhase(string phase, decimal phaseProgress, DateTime phaseStartDate, DateTime activityDate)
    {
        return new StarterProgress(
            phase,
            phaseStartDate,
            PhaseExpectedEndDate,
            OverallProgress,
            phaseProgress,
            TasksCompleted,
            TasksTotal,
            TasksOverdue,
            FormsCompleted,
            FormsTotal,
            FormsRejected,
            MilestonesAchieved,
            MilestonesTotal,
            activityDate,
            NextMilestoneDate,
            NextMilestoneName,
            EngagementScore,
            PerformanceScore);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CurrentPhase;
        yield return PhaseStartDate;
        yield return PhaseExpectedEndDate ?? DateTime.MinValue;
        yield return OverallProgress;
        yield return PhaseProgress;
        yield return TasksCompleted;
        yield return TasksTotal;
        yield return TasksOverdue;
        yield return FormsCompleted;
        yield return FormsTotal;
        yield return FormsRejected;
        yield return MilestonesAchieved;
        yield return MilestonesTotal;
        yield return LastActivityDate ?? DateTime.MinValue;
        yield return NextMilestoneDate ?? DateTime.MinValue;
        yield return NextMilestoneName ?? string.Empty;
        yield return EngagementScore ?? 0;
        yield return PerformanceScore ?? 0;
    }

    private static decimal ValidatePercentage(decimal value, string paramName)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentException($"{paramName} must be between 0 and 100", paramName);
        }

        return value;
    }

    private static int ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{paramName} cannot be negative", paramName);
        }

        return value;
    }
}
