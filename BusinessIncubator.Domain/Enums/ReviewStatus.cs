namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Review status enumeration.
/// </summary>
public enum ReviewStatus
{
    /// <summary>
    /// Review is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Submission is approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Changes are requested.
    /// </summary>
    ChangesRequested = 2,

    /// <summary>
    /// Submission is flagged for special attention.
    /// </summary>
    Flagged = 3
}