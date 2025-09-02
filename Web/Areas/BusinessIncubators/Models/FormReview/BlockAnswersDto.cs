namespace LinaSys.Web.Areas.BusinessIncubators.Models.FormReview;

/// <summary>
/// DTO for block answers.
/// </summary>
public class BlockAnswersDto
{
    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the questions and answers.
    /// </summary>
    public List<QuestionAnswerDto> Questions { get; set; } = [];
}
