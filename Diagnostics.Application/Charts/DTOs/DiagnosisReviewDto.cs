using LinaSys.Diagnostics.Domain.Enums;

namespace LinaSys.Diagnostics.Application.Charts.DTOs;

/// <summary>
/// Data transfer object for diagnosis review with charts.
/// </summary>
public class DiagnosisReviewDto
{
    /// <summary>
    /// Gets or sets the submission date of the diagnosis data.
    /// </summary>
    public DateTime SubmissionDate { get; set; }

    /// <summary>
    /// Gets or sets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the collection of charts.
    /// </summary>
    public List<DiagnosisChartDto> Charts { get; set; } = new();
}