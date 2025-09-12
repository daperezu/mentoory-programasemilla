namespace LinaSys.Web.Areas.Diagnostics.Models;

/// <summary>
/// View model for diagnosis review with charts.
/// </summary>
public class DiagnosisReviewViewModel
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
    /// Gets or sets the phase display text.
    /// </summary>
    public string PhaseDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime ReviewDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of charts.
    /// </summary>
    public List<ChartViewModel> Charts { get; set; } = new();

    /// <summary>
    /// Gets or sets the print URL.
    /// </summary>
    public string? PrintUrl { get; set; }
}