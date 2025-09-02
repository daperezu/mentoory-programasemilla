namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Report type enumeration.
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Progress report showing project advancement.
    /// </summary>
    Progress = 0,

    /// <summary>
    /// Completion report showing finished tasks and milestones.
    /// </summary>
    Completion = 1,

    /// <summary>
    /// Participation report showing participant engagement metrics.
    /// </summary>
    Participation = 2,

    /// <summary>
    /// Custom report with user-defined metrics and layout.
    /// </summary>
    Custom = 3
}