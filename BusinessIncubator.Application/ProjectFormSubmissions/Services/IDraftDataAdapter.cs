using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Services;

/// <summary>
/// Service responsible for adapting draft data to match the latest form schema version.
/// This service ensures that old draft data can be safely loaded into the current form structure.
/// </summary>
public interface IDraftDataAdapter
{
    /// <summary>
    /// Adapts draft data from a specific version to match the current form structure.
    /// This method handles:
    /// - Injecting default values for newly introduced fields
    /// - Ignoring fields that no longer exist in the current form version
    /// - Handling structural changes (grouping, nesting, data types)
    /// </summary>
    /// <param name="draft">The draft data to adapt.</param>
    /// <param name="draftVersion">The version of the draft data.</param>
    /// <param name="currentVersion">The current form schema version.</param>
    /// <param name="structure">The project knowledge structure containing the current form definition.</param>
    /// <returns>The adapted draft data that matches the current form structure.</returns>
    Task<DraftDataDto> AdaptToCurrentVersionAsync(
        DraftDataDto draft,
        int draftVersion,
        int currentVersion,
        Domain.Aggregates.BusinessIncubator.ProjectKnowledgeStructure structure);

    /// <summary>
    /// Checks if adaptation is needed based on version differences.
    /// </summary>
    /// <param name="draftVersion">The version of the draft data.</param>
    /// <param name="currentVersion">The current form schema version.</param>
    /// <returns>True if adaptation is needed.</returns>
    bool IsAdaptationNeeded(int draftVersion, int currentVersion);

    /// <summary>
    /// Gets a summary of what adaptations will be applied.
    /// </summary>
    /// <param name="draft">The draft data.</param>
    /// <param name="draftVersion">The version of the draft data.</param>
    /// <param name="currentVersion">The current form schema version.</param>
    /// <param name="structure">The current project knowledge structure.</param>
    /// <returns>A summary of adaptation changes.</returns>
    Task<AdaptationSummary> GetAdaptationSummaryAsync(
        DraftDataDto draft,
        int draftVersion,
        int currentVersion,
        Domain.Aggregates.BusinessIncubator.ProjectKnowledgeStructure structure);
}

/// <summary>
/// Summary of changes that will occur during draft adaptation.
/// </summary>
public class AdaptationSummary
{
    /// <summary>
    /// Gets or sets questions that will be added with default values.
    /// </summary>
    public List<QuestionChange> AddedQuestions { get; set; } = [];

    /// <summary>
    /// Gets or sets questions whose data will be ignored (no longer exist).
    /// </summary>
    public List<QuestionChange> RemovedQuestions { get; set; } = [];

    /// <summary>
    /// Gets or sets questions that will be adapted due to structural changes.
    /// </summary>
    public List<QuestionChange> ModifiedQuestions { get; set; } = [];

    /// <summary>
    /// Gets or sets adaptation warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Gets whether any draft data will be ignored during adaptation.
    /// </summary>
    public bool HasDataToIgnore => RemovedQuestions.Any() || ModifiedQuestions.Any(q => q.RequiresConversion);
}

/// <summary>
/// Represents a change to a question during adaptation.
/// </summary>
public class QuestionChange
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change reason.
    /// </summary>
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the change requires data conversion.
    /// </summary>
    public bool RequiresConversion { get; set; }

    /// <summary>
    /// Gets or sets the old answer type.
    /// </summary>
    public string? OldAnswerType { get; set; }

    /// <summary>
    /// Gets or sets the new answer type.
    /// </summary>
    public string? NewAnswerType { get; set; }
}
