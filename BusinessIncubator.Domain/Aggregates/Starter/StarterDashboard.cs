using LinaSys.Core.Domain.Aggregates.Dashboard;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

/// <summary>
/// Starter role dashboard.
/// </summary>
public class StarterDashboard : BaseDashboard
{
    private readonly List<StarterTask> _tasks = [];

    public StarterDashboard(string userId, string role, long projectId, DateTime createdDate)
        : base(userId, role, createdDate)
    {
        ProjectId = projectId;
        Progress = new StarterProgress();
    }

    protected StarterDashboard()
        : base()
    {
        Progress = new StarterProgress();
    }

    /// <summary>
    /// Project ID associated with this starter.
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// Starter progress information.
    /// </summary>
    public StarterProgress Progress { get; private set; }

    /// <summary>
    /// Active tasks.
    /// </summary>
    public IReadOnlyList<StarterTask> Tasks => _tasks.AsReadOnly();

    /// <summary>
    /// Mentor information.
    /// </summary>
    public MentorInfo? MentorInfo { get; private set; }

    /// <summary>
    /// Get dashboard metrics.
    /// </summary>
    /// <param name="currentTime">The current time for calculations.</param>
    public DashboardMetrics GetMetrics(DateTime currentTime)
    {
        return new StarterMetrics(
            Progress.OverallProgress,
            _tasks.Count(t => t.Status == TaskStatus.Pending),
            _tasks.Count(t => t.Status == TaskStatus.Completed),
            _tasks.Count,
            GetUnreadNotificationsCount(),
            Progress.LastActivityDate,
            Progress.GetDaysSinceStart(currentTime),
            Progress.CurrentPhase,
            Progress.FormsCompleted,
            Progress.FormsTotal,
            _tasks.Count(t => t.IsOverdue(currentTime)));
    }

    /// <summary>
    /// Get dashboard metrics.
    /// </summary>
    public override DashboardMetrics GetMetrics()
    {
        throw new NotSupportedException("Use GetMetrics(DateTime) instead.");
    }

    /// <summary>
    /// Update progress.
    /// </summary>
    public void UpdateProgress(StarterProgress progress)
    {
        Progress = progress ?? throw new ArgumentNullException(nameof(progress));
    }

    /// <summary>
    /// Update progress with specific values.
    /// </summary>
    public void UpdateProgress(
        decimal overallProgress,
        string currentPhase,
        int tasksCompleted,
        int tasksTotal,
        int formsCompleted,
        int formsTotal,
        DateTime currentTime)
    {
        Progress = new StarterProgress(
            overallProgress,
            currentPhase,
            tasksCompleted,
            tasksTotal,
            Progress.TasksOverdue,
            formsCompleted,
            formsTotal,
            Progress.MilestonesCompleted,
            Progress.MilestonesTotal,
            Progress.StartDate,
            Progress.LastActivityDate,
            Progress.GetDaysSinceStart(currentTime));
    }

    /// <summary>
    /// Add task
    /// </summary>
    public void AddTask(StarterTask? task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        _tasks.Add(task);
    }

    /// <summary>
    /// Remove task
    /// </summary>
    public void RemoveTask(long taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is not null)
        {
            _tasks.Remove(task);
        }
    }

    /// <summary>
    /// Complete task
    /// </summary>
    public void CompleteTask(long taskId, DateTime completedDate)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        task?.Complete(completedDate);

        // Update progress
        RecalculateProgress(completedDate);
    }

    /// <summary>
    /// Set mentor information
    /// </summary>
    public void SetMentorInfo(MentorInfo mentorInfo)
    {
        MentorInfo = mentorInfo;
    }

    /// <summary>
    /// Get active tasks
    /// </summary>
    public IEnumerable<StarterTask> GetActiveTasks()
    {
        return _tasks.Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress)
                     .OrderBy(t => t.Priority)
                     .ThenBy(t => t.DueDate);
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    public IEnumerable<StarterTask> GetOverdueTasks(DateTime currentTime)
    {
        return _tasks.Where(t => t.IsOverdue(currentTime))
                     .OrderBy(t => t.DueDate);
    }

    /// <summary>
    /// Get upcoming tasks
    /// </summary>
    public IEnumerable<StarterTask> GetUpcomingTasks(int days, DateTime currentTime)
    {
        var endDate = currentTime.AddDays(days);
        return _tasks.Where(t => t.Status == TaskStatus.Pending &&
                                t.DueDate.HasValue &&
                                t.DueDate.Value <= endDate)
                     .OrderBy(t => t.DueDate);
    }

    /// <summary>
    /// Recalculate progress based on tasks
    /// </summary>
    private void RecalculateProgress(DateTime currentTime)
    {
        if (_tasks.Count == 0)
        {
            return;
        }

        var completedTasks = _tasks.Count(t => t.Status == TaskStatus.Completed);
        var totalTasks = _tasks.Count;
        var progressPercentage = Math.Round((decimal)completedTasks / totalTasks * 100, 2);

        Progress = Progress.WithTaskProgress(completedTasks, totalTasks, progressPercentage, currentTime);
    }
}
