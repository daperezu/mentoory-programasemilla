namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Report status enumeration.
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Report generation is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Report is being generated.
    /// </summary>
    Generating = 1,

    /// <summary>
    /// Report generation completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Report generation failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Report has been cancelled.
    /// </summary>
    Cancelled = 4
}