using LinaSys.BusinessIncubator.Domain.ValueObjects;

namespace LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;

/// <summary>
/// Synchronization methods for ProjectQuestion entity.
/// </summary>
public partial class ProjectQuestion
{
    private readonly List<ProjectAnswerOption> _projectAnswerOptions = [];

    /// <summary>
    /// Gets or sets the last time this question was synchronized.
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Gets or sets whether help text is customized.
    /// </summary>
    public bool IsHelpTextCustomized { get; private set; }

    /// <summary>
    /// Gets or sets the help text.
    /// </summary>
    public string? HelpText { get; private set; }

    /// <summary>
    /// Gets or sets whether required status is customized.
    /// </summary>
    public bool IsRequiredCustomized { get; private set; }

    /// <summary>
    /// Gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Gets or sets whether answer options are customized.
    /// </summary>
    public bool IsAnswerOptionsCustomized { get; private set; }

    /// <summary>
    /// Synchronizes this question from its source.
    /// </summary>
    public SyncResult SyncFromSource(Dictionary<long, Question>? sourceQuestions)
    {
        var result = new SyncResult();

        // Find our source question
        Question? sourceQuestion = null;
        if (SourceQuestionId.HasValue && sourceQuestions?.ContainsKey(SourceQuestionId.Value) == true)
        {
            sourceQuestion = sourceQuestions[SourceQuestionId.Value];
        }

        // Sync question-level properties
        if (sourceQuestion is not null && !IsTextCustomized)
        {
            if (Text != sourceQuestion.Text)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Question",
                    ElementName = Text,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "Text",
                    OldValue = Text,
                    NewValue = sourceQuestion.Text
                });
                Text = sourceQuestion.Text;
            }
        }

        if (sourceQuestion is not null && !IsHelpTextCustomized && HelpText != sourceQuestion.HelpText)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Question",
                ElementName = Text,
                ChangeType = SyncChangeType.Update,
                FieldName = "HelpText",
                OldValue = HelpText,
                NewValue = sourceQuestion.HelpText
            });
            HelpText = sourceQuestion.HelpText;
        }

        if (sourceQuestion is not null && !IsRequiredCustomized && IsRequired != sourceQuestion.IsRequired)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Question",
                ElementName = Text,
                ChangeType = SyncChangeType.Update,
                FieldName = "IsRequired",
                OldValue = IsRequired.ToString(),
                NewValue = sourceQuestion.IsRequired.ToString()
            });
            IsRequired = sourceQuestion.IsRequired;
        }

        if (sourceQuestion is not null && !IsOrderCustomized && Order != sourceQuestion.Order)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Question",
                ElementName = Text,
                ChangeType = SyncChangeType.Update,
                FieldName = "Order",
                OldValue = Order.ToString(),
                NewValue = sourceQuestion.Order.ToString()
            });
            Order = sourceQuestion.Order;
        }

        if (sourceQuestion is not null && !IsAnswerTypeCustomized)
        {
            var sourceAnswerTypeStr = sourceQuestion.AnswerType;
            if (AnswerType.ToString() != sourceAnswerTypeStr)
            {
                result.AddChange(new SyncChange
                {
                    ElementId = Id,
                    ElementType = "Question",
                    ElementName = Text,
                    ChangeType = SyncChangeType.Update,
                    FieldName = "AnswerType",
                    OldValue = AnswerType.ToString(),
                    NewValue = sourceAnswerTypeStr
                });
                // Parse the answer type enum
                if (Enum.TryParse<Enums.AnswerType>(sourceAnswerTypeStr, out var newAnswerType))
                {
                    AnswerType = newAnswerType;
                }
            }
        }

        // Sync answer options if not customized
        if (sourceQuestion is not null && !IsAnswerOptionsCustomized)
        {
            // This would require syncing the ProjectAnswerOptions collection
            // Implementation depends on how answer options are stored
            SyncAnswerOptions(sourceQuestion, result);
        }

        // Handle source deletion
        if (SourceQuestionId.HasValue && sourceQuestion is null)
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Question",
                ElementName = Text,
                ChangeType = SyncChangeType.Orphaned,
                FieldName = "SourceQuestionId",
                OldValue = SourceQuestionId.ToString(),
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
        SourceQuestionId = null;
        // Keep all data but mark as orphaned
    }

    /// <summary>
    /// Checks if the question is orphaned (source deleted but data preserved).
    /// </summary>
    /// <returns>True if the question had a source that no longer exists.</returns>
    public bool IsOrphaned()
    {
        // This would need to be determined by checking if SourceQuestionId exists
        // but the source is not available. The caller needs to provide this info.
        return false; // Placeholder implementation
    }

    /// <summary>
    /// Syncs answer options from the source question.
    /// </summary>
    private void SyncAnswerOptions(Question sourceQuestion, SyncResult result)
    {
        // Clear existing options
        if (ProjectAnswerOptions.Any())
        {
            result.AddChange(new SyncChange
            {
                ElementId = Id,
                ElementType = "Question",
                ElementName = Text,
                ChangeType = SyncChangeType.Update,
                FieldName = "AnswerOptions",
                OldValue = $"{ProjectAnswerOptions.Count} options",
                NewValue = $"{sourceQuestion.AnswerOptions?.Count ?? 0} options"
            });
            ProjectAnswerOptions.Clear();
        }

        // Add new options from source
        if (sourceQuestion.AnswerOptions is not null)
        {
            foreach (var sourceOption in sourceQuestion.AnswerOptions)
            {
                // Using the existing AddProjectAnswerOption method
                AddProjectAnswerOption(
                    sourceAnswerOptionId: sourceOption.Id,
                    text: sourceOption.OptionText,
                    isTextCustomized: false,
                    score: sourceOption.Value ?? 0,
                    isScoreCustomized: false,
                    foda: Enums.FodaType.NoDefinido,
                    isFodaCustomized: false,
                    fodaExplanation: string.Empty,
                    isFodaExplanationCustomized: false,
                    odsr: Enums.OdsrType.NoDefinido,
                    isOdsrCustomized: false,
                    odsrExplanation: string.Empty,
                    isOdsrExplanationCustomized: false,
                    order: sourceOption.Order,
                    isOrderCustomized: false,
                    followUpQuestionText: string.Empty,
                    isFollowUpTextCustomized: false);
            }
        }
    }
}