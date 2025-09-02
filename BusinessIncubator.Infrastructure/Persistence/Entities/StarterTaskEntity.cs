namespace LinaSys.BusinessIncubator.Infrastructure.Persistence.Entities;

/// <summary>
/// Persistence entity for StarterTask
/// </summary>
public class StarterTaskEntity
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = "pending";
    public string? Phase { get; set; }
    public int? EstimatedDuration { get; set; }
    public int? ActualDuration { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? Metadata { get; set; }
    public string? Prerequisites { get; set; }
    public string? DependentTasks { get; set; }
    public string? RecurrenceRule { get; set; }
    public long? ParentTaskId { get; set; }
    public bool IsBlocking { get; set; }
    public bool IsAutomated { get; set; }
    public string? AutoCompleteCondition { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Domain.Aggregates.BusinessIncubator.Project? Project { get; set; }
    public StarterTaskEntity? ParentTask { get; set; }
    public ICollection<StarterTaskEntity> SubTasks { get; set; } = [];
}