using LinaSys.BusinessIncubator.Domain.ValueObjects;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Synchronization methods for ProjectSubject entity.
/// </summary>
public partial class ProjectSubject
{
    /// <summary>
    /// Gets or sets the last time this subject was synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Synchronizes this subject from its source.
    /// </summary>
    /// <returns></returns>
    public SyncResult SyncFromSource(
        Dictionary<long, Subject>? sourceSubjects,
        Dictionary<long, Question>? sourceQuestions)
    {
        var result = new SyncResult();

        // Find our source subject
        Subject? sourceSubject = null;
        if (SourceSubjectId.HasValue && sourceSubjects?.ContainsKey(SourceSubjectId.Value) == true)
        {
            sourceSubject = sourceSubjects[SourceSubjectId.Value];
        }

        // Sync subject-level properties
        if (sourceSubject is not null && !IsTitleCustomized)
        {
            if (Title != sourceSubject.Name)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Subject",
                    ElementName = Title,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Title",
                    OldValue = Title,
                    NewValue = sourceSubject.Name
                });
                Title = sourceSubject.Name;
            }
        }

        if (sourceSubject is not null && !IsOrderCustomized)
        {
            if (Order != sourceSubject.Order)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Subject",
                    ElementName = Title,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Order",
                    OldValue = Order.ToString(),
                    NewValue = sourceSubject.Order.ToString()
                });
                Order = sourceSubject.Order;
            }
        }

        // Handle source deletion
        if (SourceSubjectId.HasValue && sourceSubject is null)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Subject",
                ElementName = Title,
                ChangeType = SyncChangeType.Orphaned,
                FieldName = "SourceSubjectId",
                OldValue = SourceSubjectId.ToString(),
                NewValue = null
            });
            ClearSourceReference();
        }

        LastSyncedAt = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Clears the source reference when the source is deleted.
    /// </summary>
    public void ClearSourceReference()
    {
        SourceSubjectId = null;
        // Keep all data but mark as orphaned
    }

    /// <summary>
    /// Checks if the subject is orphaned (source deleted but data preserved).
    /// </summary>
    /// <returns>True if the subject had a source that no longer exists.</returns>
    public bool IsOrphaned()
    {
        // This would need to be determined by checking if SourceSubjectId exists
        // but the source is not available. The caller needs to provide this info.
        return false; // Placeholder implementation
    }
}