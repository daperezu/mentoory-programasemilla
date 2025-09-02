using LinaSys.BusinessIncubator.Domain.ValueObjects;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Synchronization methods for ProjectKnowledgeStructure aggregate.
/// </summary>
public partial class ProjectKnowledgeStructure
{
    /// <summary>
    /// Gets or sets the last time this structure was synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Synchronizes this structure from source structures.
    /// </summary>
    /// <returns>The result of the synchronization operation.</returns>
    public SyncResult SyncFromSources(
        KnowledgeStructure? sourceStructure,
        Dictionary<long, Module>? sourceModules,
        Dictionary<long, Topic>? sourceTopics,
        Dictionary<long, Subject>? sourceSubjects,
        Dictionary<long, Question>? sourceQuestions)
    {
        var result = new SyncResult();

        // Sync structure-level properties
        if (sourceStructure is not null && !IsNameCustomized)
        {
            if (Name != sourceStructure.Name)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Structure",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Name",
                    OldValue = Name,
                    NewValue = sourceStructure.Name
                });
                Name = sourceStructure.Name;
            }
        }

        if (sourceStructure is not null && !IsDescriptionCustomized)
        {
            if (Description != sourceStructure.Description)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Structure",
                    ElementName = Name,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Description",
                    OldValue = Description,
                    NewValue = sourceStructure.Description
                });
                Description = sourceStructure.Description;
            }
        }

        // Handle source deletion
        if (SourceKnowledgeStructureId.HasValue && sourceStructure is null)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Structure",
                ElementName = Name,
                ChangeType = SyncChangeType.Orphaned,
                FieldName = "SourceKnowledgeStructureId",
                OldValue = SourceKnowledgeStructureId.ToString(),
                NewValue = null
            });
            ClearSourceReference();
        }

        // Sync modules
        foreach (var module in _projectModules)
        {
            var moduleResult = module.SyncFromSource(sourceModules, sourceTopics, sourceSubjects, sourceQuestions);
            result.Merge(moduleResult);
        }

        LastSyncedAt = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Clears the source reference when the source is deleted.
    /// </summary>
    public void ClearSourceReference()
    {
        SourceKnowledgeStructureId = null;
        // Keep all data but mark as orphaned
    }

    /// <summary>
    /// Finds a module by its source ID.
    /// </summary>
    /// <param name="sourceModuleId">The source module ID to find.</param>
    /// <returns>The project module if found, null otherwise.</returns>
    public ProjectModule? FindModuleBySourceId(long sourceModuleId)
    {
        return _projectModules.FirstOrDefault(m => m.SourceModuleId == sourceModuleId);
    }

    /// <summary>
    /// Finds a subject by its source ID across all modules and topics.
    /// </summary>
    /// <param name="sourceSubjectId">The source subject ID to find.</param>
    /// <returns>The project subject if found, null otherwise.</returns>
    public ProjectSubject? FindSubjectBySourceId(long sourceSubjectId)
    {
        return _projectModules
            .SelectMany(m => m.ProjectTopics)
            .SelectMany(t => t.ProjectSubjects)
            .FirstOrDefault(s => s.SourceSubjectId == sourceSubjectId);
    }

    /// <summary>
    /// Finds a question by its source ID across all modules, topics, and subjects.
    /// </summary>
    /// <param name="sourceQuestionId">The source question ID to find.</param>
    /// <returns>The project question if found, null otherwise.</returns>
    public ProjectQuestion? FindQuestionBySourceId(long sourceQuestionId)
    {
        return _projectModules
            .SelectMany(m => m.ProjectTopics)
            .SelectMany(t => t.ProjectQuestions)
            .FirstOrDefault(q => q.SourceQuestionId == sourceQuestionId);
    }

    /// <summary>
    /// Checks if the structure is fully customized.
    /// </summary>
    /// <returns>True if all customizable fields are customized.</returns>
    public bool IsFullyCustomized()
    {
        return IsNameCustomized && IsDescriptionCustomized;
    }

    /// <summary>
    /// Gets the count of orphaned elements (elements whose source has been deleted).
    /// </summary>
    /// <returns>The count of orphaned elements.</returns>
    public int GetOrphanedElementsCount()
    {
        int count = 0;

        // Check if structure itself is orphaned
        if (SourceKnowledgeStructureId.HasValue && !IsNameCustomized)
        {
            // This would need to check against actual source existence
            // For now, we'll assume the caller provides this information
        }

        // Count orphaned modules
        count += _projectModules.Count(m => m.IsOrphaned());

        // Count orphaned topics
        count += _projectModules
            .SelectMany(m => m.ProjectTopics)
            .Count(t => t.IsOrphaned());

        // Count orphaned subjects
        count += _projectModules
            .SelectMany(m => m.ProjectTopics)
            .SelectMany(t => t.ProjectSubjects)
            .Count(s => s.IsOrphaned());

        // Count orphaned questions (questions are in topics, not subjects)
        count += _projectModules
            .SelectMany(m => m.ProjectTopics)
            .SelectMany(t => t.ProjectQuestions)
            .Count(q => q.IsOrphaned());

        return count;
    }

    /// <summary>
    /// Gets sync statistics for the structure.
    /// </summary>
    /// <returns>Sync statistics.</returns>
    public SyncStatistics GetSyncStatistics()
    {
        var stats = new SyncStatistics
        {
            TotalElements = 1, // The structure itself
            CustomizedElements = IsFullyCustomized() ? 1 : 0,
            LastSyncDate = LastSyncedAt
        };

        // Count modules
        stats.TotalElements += _projectModules.Count;
        stats.CustomizedElements += _projectModules.Count(m => m.IsFullyCustomized());

        // Count topics
        var topics = _projectModules.SelectMany(m => m.ProjectTopics).ToList();
        stats.TotalElements += topics.Count;
        stats.CustomizedElements += topics.Count(t => t.IsFullyCustomized());

        // Count subjects
        var subjects = topics.SelectMany(t => t.ProjectSubjects).ToList();
        stats.TotalElements += subjects.Count;
        stats.CustomizedElements += subjects.Count(s => s.IsFullyCustomized());

        // Count questions (questions are in topics, not subjects)
        var questions = topics.SelectMany(t => t.ProjectQuestions).ToList();
        stats.TotalElements += questions.Count;
        stats.CustomizedElements += questions.Count(q => q.IsFullyCustomized());

        stats.SyncedElements = stats.TotalElements - stats.CustomizedElements - stats.OrphanedElements;
        stats.OrphanedElements = GetOrphanedElementsCount();

        return stats;
    }
}