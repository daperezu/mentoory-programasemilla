namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Enumeration for batch user registration status.
/// </summary>
public enum BatchUserRegistrationStatus
{
    /// <summary>
    /// The batch registration is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The batch registration is currently being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// The batch registration has been completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The batch registration has failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The batch registration has been partially completed with some failures.
    /// </summary>
    PartiallyCompleted = 4,
}
