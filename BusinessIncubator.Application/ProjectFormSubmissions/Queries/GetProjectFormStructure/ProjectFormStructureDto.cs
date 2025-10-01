namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetProjectFormStructure;

/// <summary>
/// DTO representing the structure of a project form.
/// </summary>
public sealed class ProjectFormStructureDto
{
    /// <summary>
    /// Gets or sets the form ID.
    /// </summary>
    public long FormId { get; set; }

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the blocks with their questions.
    /// </summary>
    public List<FormBlockDto> Blocks { get; set; } = [];
}

/// <summary>
/// DTO representing a form block.
/// </summary>
public sealed class FormBlockDto
{
    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the questions in this block.
    /// </summary>
    public List<FormQuestionDto> Questions { get; set; } = [];
}

/// <summary>
/// DTO representing a form question.
/// </summary>
public sealed class FormQuestionDto
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the help text.
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets the order within the block.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the topic ID if linked to a topic.
    /// </summary>
    public long? TopicId { get; set; }

    /// <summary>
    /// Gets or sets the topic name if linked.
    /// </summary>
    public string? TopicName { get; set; }

    /// <summary>
    /// Gets or sets the answer options for multiple choice questions.
    /// </summary>
    public List<AnswerOptionDto> AnswerOptions { get; set; } = [];
}

/// <summary>
/// DTO representing an answer option.
/// </summary>
public sealed class AnswerOptionDto
{
    /// <summary>
    /// Gets or sets the option ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the option text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the option value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score for this option.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    public int Order { get; set; }
}
