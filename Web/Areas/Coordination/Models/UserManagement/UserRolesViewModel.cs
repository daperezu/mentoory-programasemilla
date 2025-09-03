using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

/// <summary>
/// Enum for role assignment actions.
/// </summary>
public enum RoleAssignmentAction
{
    /// <summary>
    /// Add the role to selected users.
    /// </summary>
    Add,

    /// <summary>
    /// Remove the role from selected users.
    /// </summary>
    Remove
}

/// <summary>
/// View model for managing user roles.
/// </summary>
public class UserRolesViewModel
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current roles assigned to the user.
    /// </summary>
    public List<string> CurrentRoles { get; set; } = new();

    /// <summary>
    /// Gets or sets the available roles that can be assigned.
    /// </summary>
    public List<RoleSelectionViewModel> AvailableRoles { get; set; } = new();

    /// <summary>
    /// Gets or sets the selected roles for the user.
    /// </summary>
    [Display(Name = "Roles Asignados")]
    public List<string> SelectedRoles { get; set; } = new();
}

/// <summary>
/// View model for role selection.
/// </summary>
public class RoleSelectionViewModel
{
    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this role is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this role can be modified by the current user.
    /// </summary>
    public bool CanModify { get; set; } = true;
}

/// <summary>
/// View model for batch role assignment.
/// </summary>
public class BatchRoleAssignmentViewModel
{
    /// <summary>
    /// Gets or sets the user IDs to assign roles to.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar al menos un usuario")]
    public List<string> UserIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the role to assign.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar un rol")]
    [Display(Name = "Rol a Asignar")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assignment action (add or remove).
    /// </summary>
    [Required]
    [Display(Name = "Acción")]
    public RoleAssignmentAction Action { get; set; }
}
