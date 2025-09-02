namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetSyncStatus;

/// <summary>
/// DTO containing synchronization status information for a project.
/// </summary>
public class SyncStatusDto
{
    /// <summary>
    /// Gets or sets the total number of elements in the structure.
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
    /// Gets or sets the list of pending changes.
    /// </summary>
    public List<PendingChangeDto> PendingChanges { get; set; } = [];

    /// <summary>
    /// Gets the sync percentage.
    /// </summary>
    public double SyncPercentage => TotalElements > 0
        ? (double)SyncedElements / TotalElements * 100
        : 0;

    /// <summary>
    /// Gets the customization percentage.
    /// </summary>
    public double CustomizedPercentage => TotalElements > 0
        ? (double)CustomizedElements / TotalElements * 100
        : 0;
}

/// <summary>
/// DTO representing a pending change.
/// </summary>
public class PendingChangeDto
{
    /// <summary>
    /// Gets or sets the element ID.
    /// </summary>
    public long ElementId { get; set; }

    /// <summary>
    /// Gets or sets the element type.
    /// </summary>
    public string ElementType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the element name.
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field that has changes.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public string? CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the source value.
    /// </summary>
    public string? SourceValue { get; set; }

    /// <summary>
    /// Gets or sets the change description.
    /// </summary>
    public string ChangeDescription { get; set; } = string.Empty;
}