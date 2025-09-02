namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Enumeration for project invitation status.
/// </summary>
public enum ProjectInvitationStatus
{
    /// <summary>
    /// The invitation is pending acceptance.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The invitation has been accepted by the user.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// The invitation has expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// The invitation has been revoked.
    /// </summary>
    Revoked = 3,

    /// <summary>
    /// The invitation has been declined by the user.
    /// </summary>
    Declined = 4,
}
