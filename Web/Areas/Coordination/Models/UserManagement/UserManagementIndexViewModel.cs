namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class UserManagementIndexViewModel
{
    public bool CanCreateUsers { get; set; }
    public bool CanEditUsers { get; set; }
    public string CurrentUserRole { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public long? IncubatorId { get; set; }
    public long? ProjectId { get; set; }
}
