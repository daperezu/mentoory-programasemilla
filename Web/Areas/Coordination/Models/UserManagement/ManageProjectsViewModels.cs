using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

/// <summary>
/// ViewModel for the main ManageProjects page.
/// </summary>
public class ManageProjectsViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the current user can add the user to projects.
    /// </summary>
    public bool CanAddToProjects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user can remove the user from projects.
    /// </summary>
    public bool CanRemoveFromProjects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user can change roles.
    /// </summary>
    public bool CanChangeRoles { get; set; }

    /// <summary>
    /// Gets or sets the current user's role.
    /// </summary>
    public string CurrentUserRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current user's incubator ID (for scoping).
    /// </summary>
    public long? CurrentUserIncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the current user's project ID (for scoping).
    /// </summary>
    public long? CurrentUserProjectId { get; set; }
}

/// <summary>
/// ViewModel for a single project assignment row in the DataTable.
/// </summary>
public class UserProjectAssignmentListItemViewModel
{
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key.
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator identifier.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator key.
    /// </summary>
    public string IncubatorKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role display name (Spanish).
    /// </summary>
    public string RoleDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the access is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the status display text (Spanish).
    /// </summary>
    public string StatusDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the user was assigned to the project.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the formatted creation date.
    /// </summary>
    public string CreatedAtDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last synchronization timestamp.
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}

/// <summary>
/// ViewModel for adding a user to a new project.
/// </summary>
public class AddToProjectViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "El identificador del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected incubator ID.
    /// </summary>
    [Required(ErrorMessage = "La incubadora es requerida")]
    [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar una incubadora válida")]
    [Display(Name = "Incubadora")]
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the selected project ID.
    /// </summary>
    [Required(ErrorMessage = "El proyecto es requerido")]
    [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar un proyecto válido")]
    [Display(Name = "Proyecto")]
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the selected role.
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    [MinLength(1, ErrorMessage = "El rol es requerido")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for changing a user's role in a project.
/// </summary>
public class ChangeProjectRoleViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "El identificador del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    [Required(ErrorMessage = "El identificador del proyecto es requerido")]
    [Range(1, long.MaxValue, ErrorMessage = "El identificador del proyecto no es válido")]
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name (for display).
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current role.
    /// </summary>
    public string CurrentRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new role.
    /// </summary>
    [Required(ErrorMessage = "El nuevo rol es requerido")]
    [MinLength(1, ErrorMessage = "El nuevo rol es requerido")]
    [Display(Name = "Nuevo Rol")]
    public string NewRole { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for removing a user from a project (confirmation).
/// </summary>
public class RemoveFromProjectViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "El identificador del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    [Required(ErrorMessage = "El identificador del proyecto es requerido")]
    [Range(1, long.MaxValue, ErrorMessage = "El identificador del proyecto no es válido")]
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name (for display in confirmation).
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's full name (for display in confirmation).
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;
}
