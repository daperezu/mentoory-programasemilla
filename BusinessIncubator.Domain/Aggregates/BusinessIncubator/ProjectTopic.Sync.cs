using LinaSys.BusinessIncubator.Domain.ValueObjects;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Synchronization methods for ProjectTopic entity.
/// </summary>
public partial class ProjectTopic
{
    /// <summary>
    /// Gets or sets the last time this topic was synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Synchronizes this topic from its source.
    /// </summary>
    /// <returns></returns>
    public SyncResult SyncFromSource(
        Dictionary<long, Topic>? sourceTopics,
        Dictionary<long, Subject>? sourceSubjects,
        Dictionary<long, Question>? sourceQuestions)
    {
        var result = new SyncResult();

        // Find our source topic
        Topic? sourceTopic = null;
        if (SourceTopicId.HasValue && sourceTopics?.ContainsKey(SourceTopicId.Value) == true)
        {
            sourceTopic = sourceTopics[SourceTopicId.Value];
        }

        // Sync topic-level properties
        if (sourceTopic is not null && !IsNameCustomized)
        {
            if (Name != sourceTopic.Name)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Topic",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Name",
                    OldValue = Name,
                    NewValue = sourceTopic.Name
                });
                Name = sourceTopic.Name;
            }
        }

        if (sourceTopic is not null && !IsOrderCustomized)
        {
            if (Order != sourceTopic.Order)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Topic",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Order",
                    OldValue = Order.ToString(),
                    NewValue = sourceTopic.Order.ToString()
                });
                Order = sourceTopic.Order;
            }
        }

        // Handle source deletion
        if (SourceTopicId.HasValue && sourceTopic is null)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Topic",
                ElementName = Name,
                ChangeType = SyncChangeType.Orphaned,
                FieldName = "SourceTopicId",
                OldValue = SourceTopicId.ToString(),
                NewValue = null
            });
            ClearSourceReference();
        }

        // Sync subjects
        foreach (var subject in _projectSubjects)
        {
            var subjectResult = subject.SyncFromSource(sourceSubjects, sourceQuestions);
            result.Merge(subjectResult);
        }

        LastSyncedAt = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Clears the source reference when the source is deleted.
    /// </summary>
    public void ClearSourceReference()
    {
        SourceTopicId = null;
        // Keep all data but mark as orphaned
    }

    /// <summary>
    /// Checks if the topic is orphaned (source deleted but data preserved).
    /// </summary>
    /// <returns>True if the topic had a source that no longer exists.</returns>
    public bool IsOrphaned()
    {
        // This would need to be determined by checking if SourceTopicId exists
        // but the source is not available. The caller needs to provide this info.
        return false; // Placeholder implementation
    }

    /// <summary>
    /// Finds a subject by its source ID.
    /// </summary>
    /// <param name="sourceSubjectId">The source subject ID to find.</param>
    /// <returns>The project subject if found, null otherwise.</returns>
    public ProjectSubject? FindSubjectBySourceId(long sourceSubjectId)
    {
        return _projectSubjects.FirstOrDefault(s => s.SourceSubjectId == sourceSubjectId);
    }
}