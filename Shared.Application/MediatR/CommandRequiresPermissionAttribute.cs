namespace LinaSys.Shared.Application.MediatR;

/// <summary>
/// Attribute to mark commands and queries that require specific permissions.
/// Used by the AuthorizationBehavior to enforce access control.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandRequiresPermissionAttribute"/> class.
/// </remarks>
/// <param name="permissionType">The type of permission required.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CommandRequiresPermissionAttribute(PermissionType permissionType) : Attribute
{

    /// <summary>
    /// Gets the type of permission required.
    /// </summary>
    public PermissionType PermissionType { get; } = permissionType;

    /// <summary>
    /// Gets or sets the specific roles required (optional).
    /// </summary>
    public string[]? RequiredRoles { get; set; }

    /// <summary>
    /// Gets or sets whether to check project-level access (optional).
    /// </summary>
    public bool RequiresProjectAccess { get; set; }

    /// <summary>
    /// Gets or sets whether to check business incubator-level access (optional).
    /// </summary>
    public bool RequiresBusinessIncubatorAccess { get; set; }

    /// <summary>
    /// Gets or sets the name of the property containing the resource ID to check (optional).
    /// </summary>
    public string? ResourceIdProperty { get; set; }

    /// <summary>
    /// Gets or sets the name of the property containing the project external ID (optional).
    /// </summary>
    public string? ProjectExternalIdProperty { get; set; }

    /// <summary>
    /// Gets or sets the name of the property containing the business incubator external ID (optional).
    /// </summary>
    public string? BusinessIncubatorExternalIdProperty { get; set; }
}
