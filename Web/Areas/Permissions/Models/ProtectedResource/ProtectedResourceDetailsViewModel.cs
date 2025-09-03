using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Permissions.Models.ProtectedResource;

public class ProtectedResourceDetailsViewModel
{
    public long Id { get; set; }

    public Guid ExternalId { get; set; }

    public int ResourceType { get; set; }

    public string ResourceTypeName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public IEnumerable<UserPermissionViewModel> UserPermissions { get; set; } = [];

    public IEnumerable<RolePermissionViewModel> RolePermissions { get; set; } = [];

    // For adding new permissions
    [Display(Name = "User ID")]
    public string? NewUserId { get; set; }

    [Display(Name = "Role")]
    public string? NewRole { get; set; }

    // Form models for granting access
    public GrantUserAccessViewModel GrantUserAccess { get; set; } = new();

    public GrantRoleAccessViewModel GrantRoleAccess { get; set; } = new();
}

public class UserPermissionViewModel
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}

public class RolePermissionViewModel
{
    public long Id { get; set; }

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
