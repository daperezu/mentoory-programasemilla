using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public class StarterTask : Entity
{
    public StarterTask(
        long projectId,
        string assignedToUserId,
        string title,
        string description,
        DateTime createdDate,
        string type = "general",
        string priority = "normal",
        DateTime? dueDate = null,
        string? assignedBy = null,
        string? category = null)
        : this()
    {
        ProjectId = projectId;
        AssignedToUserId = assignedToUserId ?? throw new ArgumentNullException(nameof(assignedToUserId));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Type = type;
        Priority = priority;
        DueDate = dueDate;
        AssignedBy = assignedBy;
        Category = category;
        CreatedDate = createdDate;
        ModifiedDate = createdDate;
    }

    protected StarterTask()
    {
        AssignedToUserId = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        Type = "general";
        Status = TaskStatus.Pending;
        Priority = "normal";
    }

    public long ProjectId { get; private set; }
    public string AssignedToUserId { get; private set; }
    public string UserId => AssignedToUserId; // Alias for compatibility
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Type { get; private set; }
    public string TaskType => Type; // Alias for compatibility
    public TaskStatus Status { get; private set; }
    public string Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? AssignedBy { get; private set; }
    public string? Category { get; private set; }
    public string? CompletionNotes { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    public void Complete(DateTime completedDate, string? completionNotes = null)
    {
        if (Status == TaskStatus.Completed)
        {
            throw new InvalidOperationException("La tarea ya está completada");
        }

        Status = TaskStatus.Completed;
        CompletedDate = completedDate;
        CompletionNotes = completionNotes;
        ModifiedDate = completedDate;
    }

    public void MarkAsCompleted(DateTime completedDate, string? completionNotes = null)
    {
        Status = TaskStatus.Completed;
        CompletedDate = completedDate;
        CompletionNotes = completionNotes;
        ModifiedDate = completedDate;
    }

    public void StartProgress(DateTime startedDate)
    {
        if (Status != TaskStatus.Pending)
        {
            throw new InvalidOperationException("Solo se pueden iniciar tareas pendientes");
        }

        Status = TaskStatus.InProgress;
        ModifiedDate = startedDate;
    }

    public void UpdateDueDate(DateTime? dueDate, DateTime modifiedDate)
    {
        DueDate = dueDate;
        ModifiedDate = modifiedDate;
    }

    public void UpdatePriority(string priority, DateTime modifiedDate)
    {
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        ModifiedDate = modifiedDate;
    }

    public bool IsOverdue()
    {
        return IsOverdue(DateTime.UtcNow);
    }

    public bool IsOverdue(DateTime currentTime)
    {
        return Status != TaskStatus.Completed &&
               DueDate.HasValue &&
               DueDate.Value < currentTime;
    }

    public int GetDaysUntilDue()
    {
        return GetDaysUntilDue(DateTime.UtcNow);
    }

    public int GetDaysUntilDue(DateTime currentTime)
    {
        if (!DueDate.HasValue || Status == TaskStatus.Completed)
        {
            return 0;
        }

        var days = (DueDate.Value - currentTime).Days;
        return days;
    }
}