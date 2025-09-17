using System;
using System.Collections.Generic;
using LinaSys.UserProjectDiagnosis.Domain.Aggregates.FormStructureBlock;

namespace LinaSys.UserProjectDiagnosis.Application.Queries.GetDiagnosisChartData;

/// <summary>
/// DTO for diagnosis review with charts.
/// </summary>
public class DiagnosisReviewDto
{
    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the approval date.
    /// </summary>
    public DateTime ApprovalDate { get; set; }

    /// <summary>
    /// Gets or sets the question phase.
    /// </summary>
    public QuestionPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the list of charts.
    /// </summary>
    public List<DiagnosisChartDto> Charts { get; set; } = new();
}

/// <summary>
/// DTO for a single diagnosis chart.
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
    /// Gets or sets the question scores.
    /// </summary>
    public List<QuestionScoreDto> Scores { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum score.
    /// </summary>
    public decimal MaxScore { get; set; }
}

/// <summary>
/// DTO for a question score.
/// </summary>
public class QuestionScoreDto
{
    /// <summary>
    /// Gets or sets the label (e.g., "6.2").
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