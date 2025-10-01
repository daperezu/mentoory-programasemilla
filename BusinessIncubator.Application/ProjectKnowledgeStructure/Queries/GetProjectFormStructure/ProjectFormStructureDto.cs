namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;

/// <summary>
/// DTO for the project form structure with questions.
/// </summary>
public sealed class ProjectFormStructureDto
{
    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the project knowledge structure ID.
    /// </summary>
    public long ProjectKnowledgeStructureId { get; set; }

    /// <summary>
    /// Gets or sets the blocks in the form.
    /// </summary>
    public List<ProjectFormBlockDto> Blocks { get; set; } = [];
}

/// <summary>
/// DTO for a project form block.
/// </summary>
public sealed class ProjectFormBlockDto
{
    /// <summary>
    /// Gets or sets the block ID.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the questions in the block.
    /// </summary>
    public List<ProjectFormQuestionDto> Questions { get; set; } = [];
}

/// <summary>
/// DTO for a project form question.
/// </summary>
public sealed class ProjectFormQuestionDto
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
    /// Gets or sets the help text.
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    public int AnswerType { get; set; }

    /// <summary>
    /// Gets or sets the answer type name.
    /// </summary>
    public string AnswerTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the topic ID if mapped.
    /// </summary>
    public long? TopicId { get; set; }

    /// <summary>
    /// Gets or sets the module info.
    /// </summary>
    public ProjectModuleInfoDto? ModuleInfo { get; set; }

    /// <summary>
    /// Gets or sets the topic info.
    /// </summary>
    public ProjectTopicInfoDto? TopicInfo { get; set; }

    /// <summary>
    /// Gets or sets the answer options.
    /// </summary>
    public List<ProjectFormAnswerOptionDto> AnswerOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this question is used for diagnosis.
    /// </summary>
    public bool IsUsedForDiagnosis { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this question is used for mentoring plan.
    /// </summary>
    public bool IsUsedForMentoringPlan { get; set; }

    /// <summary>
    /// Gets or sets the phase this question applies to.
    /// </summary>
    public int AppliesToPhase { get; set; }
}

/// <summary>
/// DTO for answer options.
/// </summary>
public sealed class ProjectFormAnswerOptionDto
{
    /// <summary>
    /// Gets or sets the answer option ID.
    /// </summary>
    public long AnswerOptionId { get; set; }

    /// <summary>
    /// Gets or sets the option text.
    /// </summary>
    public string OptionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this option requires follow-up.
    /// </summary>
    public bool RequiresFollowUp { get; set; }

    /// <summary>
    /// Gets or sets the follow-up question text.
    /// </summary>
    public string? FollowUpQuestionText { get; set; }
}

/// <summary>
/// Module information DTO.
/// </summary>
public sealed class ProjectModuleInfoDto
{
    /// <summary>
    /// Gets or sets the module ID.
    /// </summary>
    public long? ModuleId { get; set; }

    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
}

/// <summary>
/// Topic information DTO.
/// </summary>
public sealed class ProjectTopicInfoDto
{
    /// <summary>
    /// Gets or sets the topic ID.
    /// </summary>
    public long? TopicId { get; set; }

    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string TopicName { get; set; } = string.Empty;
}