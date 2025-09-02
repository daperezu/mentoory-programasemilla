namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class UserListItemWithRolesViewModel : UserListItemViewModel
{
    public List<string> Roles { get; set; } = new();
    public string IncubatorName { get; set; } = string.Empty;
    public List<string> ProjectNames { get; set; } = new();

    // Email verification status
    public bool EmailVerified { get; set; }

    // Invitation tracking
    public string? InvitationStatus { get; set; } // "pending", "accepted", "expired", null
    public DateTime? InvitationExpiresAt { get; set; }

    // For status column rendering
    public string Status => GetStatusValue();

    private string GetStatusValue()
    {
        if (!string.IsNullOrEmpty(InvitationStatus))
        {
            return InvitationStatus;
        }

        return IsActive ? "active" : "inactive";
    }
}
