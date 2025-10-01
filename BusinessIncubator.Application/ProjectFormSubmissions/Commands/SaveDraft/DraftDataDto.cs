namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveDraft;

/// <summary>
/// Represents the draft data structure for form submissions.
/// </summary>
public class DraftDataDto
{
    /// <summary>
    /// Gets or sets the form version to handle form structure changes.
    /// </summary>
    public int FormVersion { get; set; }

    /// <summary>
    /// Gets or sets when the draft was last saved.
    /// </summary>
    public DateTime LastSavedAt { get; set; }

    /// <summary>
    /// Gets or sets the current block index in the wizard.
    /// </summary>
    public int CurrentBlockIndex { get; set; }

    /// <summary>
    /// Gets or sets the collection of block responses.
    /// </summary>
    public List<BlockResponseDto> BlockResponses { get; set; } = [];

    /// <summary>
    /// Gets or sets the total progress percentage.
    /// </summary>
    public decimal ProgressPercentage { get; set; }
}

/// <summary>
/// Represents responses for a single block.
/// </summary>
public class BlockResponseDto
{
    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name for reference.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this block is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the question responses in this block.
    /// </summary>
    public List<QuestionResponseDto> QuestionResponses { get; set; } = [];
}

/// <summary>
/// Represents a response to a single question with simplified structure.
/// </summary>
public class QuestionResponseDto
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question text for reference.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets the answer as a string.
    /// For choice questions: comma-separated option IDs.
    /// For other types: string representation of the value.
    /// </summary>
    public string? Answer { get; set; }

    /// <summary>
    /// Gets or sets the follow-up answer if applicable.
    /// </summary>
    public string? FollowUpAnswer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this question has been answered.
    /// </summary>
    public bool IsAnswered { get; set; }

    /// <summary>
    /// Gets or sets module information for tracking.
    /// </summary>
    public ModuleInfoDto? ModuleInfo { get; set; }

    /// <summary>
    /// Gets or sets topic information for tracking.
    /// </summary>
    public TopicInfoDto? TopicInfo { get; set; }
}

/// <summary>
/// Module information for answer tracking.
/// </summary>
public class ModuleInfoDto
{
    public long? ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
}

/// <summary>
/// Topic information for answer tracking.
/// </summary>
public class TopicInfoDto
{
    public long? TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
}
