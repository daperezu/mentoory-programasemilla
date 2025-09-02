using System.ComponentModel.DataAnnotations;

namespace LinaSys.Web.Areas.Permissions.Models.ProtectedResource;

public class GrantUserAccessViewModel
{
    public long ResourceId { get; set; }

    [Required(ErrorMessage = "User ID is required.")]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = string.Empty;
}

public class GrantRoleAccessViewModel
{
    public long ResourceId { get; set; }

    [Required(ErrorMessage = "Role ID is required.")]
    [Display(Name = "Role ID")]
    public string Role { get; set; } = string.Empty;
}
