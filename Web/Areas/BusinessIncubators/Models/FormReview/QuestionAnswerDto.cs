namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// DTO for question and answer.
/// </summary>
public class QuestionAnswerDto
{
    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    public string AnswerType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the answer text.
    /// </summary>
    public string AnswerText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the question was answered.
    /// </summary>
    public bool IsAnswered { get; set; }

    /// <summary>
    /// Gets or sets the module name if applicable.
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// Gets or sets the topic name if applicable.
    /// </summary>
    public string? TopicName { get; set; }
}
