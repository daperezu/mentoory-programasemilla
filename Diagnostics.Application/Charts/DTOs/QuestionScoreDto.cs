namespace LinaSys.Diagnostics.Application.Charts.DTOs;

/// <summary>
/// Data transfer object for a question score.
/// </summary>
public class QuestionScoreDto
{
    /// <summary>
    /// Gets or sets the label (format: "blockId.internalId").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the question text.
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source ("Starter" or "Coordinator").
    /// </summary>
    public string Source { get; set; } = string.Empty;
}