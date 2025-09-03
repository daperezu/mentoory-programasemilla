using LinaSys.BusinessIncubator.Domain.ValueObjects;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Synchronization methods for ProjectModule entity.
/// </summary>
public partial class ProjectModule
{
    /// <summary>
    /// Gets or sets the last time this module was synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Synchronizes this module from its source.
    /// </summary>
    public SyncResult SyncFromSource(
        Dictionary<long, Module>? sourceModules,
        Dictionary<long, Topic>? sourceTopics,
        Dictionary<long, Subject>? sourceSubjects,
        Dictionary<long, Question>? sourceQuestions)
    {
        var result = new SyncResult();

        // Find our source module
        Module? sourceModule = null;
        if (SourceModuleId.HasValue && sourceModules?.ContainsKey(SourceModuleId.Value) == true)
        {
            sourceModule = sourceModules[SourceModuleId.Value];
        }

        // Sync module-level properties
        if (sourceModule is not null && !IsNameCustomized)
        {
            if (Name != sourceModule.Name)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Module",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Name",
                    OldValue = Name,
                    NewValue = sourceModule.Name
                });
                Name = sourceModule.Name;
            }
        }

        if (sourceModule is not null && !IsOrderCustomized)
        {
            if (Order != sourceModule.Order)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Module",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Order",
                    OldValue = Order.ToString(),
                    NewValue = sourceModule.Order.ToString()
                });
                Order = sourceModule.Order;
            }
        }

        // Handle source deletion
        if (SourceModuleId.HasValue && sourceModule is null)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Module",
                ElementName = Name,
                ChangeType = SyncChangeType.Orphaned,
                FieldName = "SourceModuleId",
                OldValue = SourceModuleId.ToString(),
                NewValue = null
            });
            ClearSourceReference();
        }

        // Sync topics
        foreach (var topic in _projectTopics)
        {
            var topicResult = topic.SyncFromSource(sourceTopics, sourceSubjects, sourceQuestions);
            result.Merge(topicResult);
        }

        LastSyncedAt = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Clears the source reference when the source is deleted.
    /// </summary>
    public void ClearSourceReference()
    {
        SourceModuleId = null;
        // Keep all data but mark as orphaned
    }

    /// <summary>
    /// Checks if the module is orphaned (source deleted but data preserved).
    /// </summary>
    /// <returns>True if the module had a source that no longer exists.</returns>
    public bool IsOrphaned()
    {
        // This would need to be determined by checking if SourceModuleId exists
        // but the source is not available. The caller needs to provide this info.
        return false; // Placeholder implementation
    }

    /// <summary>
    /// Finds a topic by its source ID.
    /// </summary>
    /// <param name="sourceTopicId">The source topic ID to find.</param>
    /// <returns>The project topic if found, null otherwise.</returns>
    public ProjectTopic? FindTopicBySourceId(long sourceTopicId)
    {
        return _projectTopics.FirstOrDefault(t => t.SourceTopicId == sourceTopicId);
    }
}