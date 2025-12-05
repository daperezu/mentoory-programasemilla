namespace LinaSys.BusinessIncubator.Domain.ValueObjects;

/// <summary>
/// Types of changes that can occur during synchronization.
/// </summary>
public enum SyncChangeType
{
    /// <summary>
    /// An existing element was updated.
    /// </summary>
    Update,

    /// <summary>
    /// A new element was added.
    /// </summary>
    Add,

    /// <summary>
    /// An element was deleted.
    /// </summary>
    Delete,

    /// <summary>
    /// An element's source was deleted but the element was preserved.
    /// </summary>
    Orphaned
}

/// <summary>
/// Represents the result of a synchronization operation.
/// </summary>
public class SyncResult
{
    private readonly List<SyncChange> _changes = [];

    /// <summary>
    /// Gets the list of changes made during synchronization.
    /// </summary>
    public IReadOnlyList<SyncChange> Changes => _changes.AsReadOnly();

    /// <summary>
    /// Gets the total number of changes.
    /// </summary>
    public int TotalChanges => _changes.Count;

    /// <summary>
    /// Gets the number of updates.
    /// </summary>
    public int UpdateCount => _changes.Count(c => c.ChangeType == SyncChangeType.Update);

    /// <summary>
    /// Gets the number of additions.
    /// </summary>
    public int AddCount => _changes.Count(c => c.ChangeType == SyncChangeType.Add);

    /// <summary>
    /// Gets the number of deletions.
    /// </summary>
    public int DeleteCount => _changes.Count(c => c.ChangeType == SyncChangeType.Delete);

    /// <summary>
    /// Gets the number of orphaned elements.
    /// </summary>
    public int OrphanedCount => _changes.Count(c => c.ChangeType == SyncChangeType.Orphaned);

    /// <summary>
    /// Gets a value indicating whether indicates whether any changes were made.
    /// </summary>
    public bool HasChanges => _changes.Any();

    /// <summary>
    /// Adds a change to the result.
    /// </summary>
    /// <param name="change">The change to add.</param>
    public void AddChange(SyncChange change)
    {
        if (change is null)
        {
            throw new ArgumentNullException(nameof(change));
        }

        _changes.Add(change);
    }

    /// <summary>
    /// Merges another sync result into this one.
    /// </summary>
    /// <param name="other">The other sync result to merge.</param>
    public void Merge(SyncResult other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        _changes.AddRange(other._changes);
    }

    /// <summary>
    /// Gets changes grouped by element type.
    /// </summary>
    /// <returns>Dictionary of element type to list of changes.</returns>
    public Dictionary<string, List<SyncChange>> GetChangesByElementType()
    {
        return _changes
            .GroupBy(c => c.ElementType)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets changes grouped by change type.
    /// </summary>
    /// <returns>Dictionary of change type to list of changes.</returns>
    public Dictionary<SyncChangeType, List<SyncChange>> GetChangesByType()
    {
        return _changes
            .GroupBy(c => c.ChangeType)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}

/// <summary>
/// Represents a single change made during synchronization.
/// </summary>
public class SyncChange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncChange"/> class.
    /// </summary>
    /// <param name="timestamp">The timestamp when the change was detected.</param>
    public SyncChange(DateTime timestamp)
    {
        Timestamp = timestamp;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncChange"/> class.
    /// </summary>
    public SyncChange()
    {
    }

    /// <summary>
    /// Gets or sets the ID of the element that changed.
    /// </summary>
    public long ElementId { get; set; }

    /// <summary>
    /// Gets or sets the type of element (e.g., "Module", "Topic", "Question").
    /// </summary>
    public string ElementType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the element.
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change.
    /// </summary>
    public SyncChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets the name of the field that changed (for updates).
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the old value (for updates).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Gets or sets the new value (for updates).
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the change was detected.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Represents synchronization statistics.
/// </summary>
public class SyncStatistics
{
    /// <summary>
    /// Gets or sets the total number of elements.
    /// </summary>
    public int TotalElements { get; set; }

    /// <summary>
    /// Gets or sets the number of synced elements.
    /// </summary>
    public int SyncedElements { get; set; }

    /// <summary>
    /// Gets or sets the number of customized elements.
    /// </summary>
    public int CustomizedElements { get; set; }

    /// <summary>
    /// Gets or sets the number of orphaned elements.
    /// </summary>
    public int OrphanedElements { get; set; }

    /// <summary>
    /// Gets or sets the last sync date.
    /// </summary>
    public DateTime? LastSyncDate { get; set; }

    /// <summary>
    /// Gets the percentage of elements that are synced.
    /// </summary>
    public double SyncPercentage => TotalElements > 0
        ? (double)SyncedElements / TotalElements * 100
        : 0;

    /// <summary>
    /// Gets the percentage of elements that are customized.
    /// </summary>
    public double CustomizedPercentage => TotalElements > 0
        ? (double)CustomizedElements / TotalElements * 100
        : 0;
}