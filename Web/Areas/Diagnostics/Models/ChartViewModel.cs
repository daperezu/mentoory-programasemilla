namespace LinaSys.Web.Areas.Diagnostics.Models;

/// <summary>
/// View model for a single chart.
/// </summary>
public class ChartViewModel
{
    /// <summary>
    /// Gets or sets the block identifier.
    /// </summary>
    public string BlockId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block name.
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chart element identifier.
    /// </summary>
    public string ChartElementId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pre-serialized chart data for JavaScript.
    /// </summary>
    public string ChartDataJson { get; set; } = string.Empty;
}