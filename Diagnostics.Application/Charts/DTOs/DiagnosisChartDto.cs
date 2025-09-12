namespace LinaSys.Diagnostics.Application.Charts.DTOs;

/// <summary>
/// Data transfer object for a single diagnosis chart.
/// </summary>
public class DiagnosisChartDto
{
    /// <summary>
    /// Gets or sets the block identifier.
    /// </summary>
    public long BlockId { get; set; }

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of question scores.
    /// </summary>
    public List<QuestionScoreDto> Scores { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum possible score.
    /// </summary>
    public decimal MaxScore { get; set; }
}