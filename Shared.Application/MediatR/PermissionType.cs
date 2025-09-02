namespace LinaSys.Shared.Application.MediatR;

/// <summary>
/// Defines the types of permissions that can be required.
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// No specific permission required (authentication only).
    /// </summary>
    None,

    /// <summary>
    /// Read access to the resource.
    /// </summary>
    Read,

    /// <summary>
    /// Write access to the resource.
    /// </summary>
    Write,

    /// <summary>
    /// Delete access to the resource.
    /// </summary>
    Delete,

    /// <summary>
    /// Administrative access to the resource.
    /// </summary>
    Admin,

    /// <summary>
    /// Project coordinator access.
    /// </summary>
    ProjectCoordinator,

    /// <summary>
    /// Project participant access.
    /// </summary>
    ProjectParticipant,

    /// <summary>
    /// Business incubator administrator access.
    /// </summary>
    BusinessIncubatorAdmin,

    /// <summary>
    /// System administrator access.
    /// </summary>
    SystemAdmin,
}
