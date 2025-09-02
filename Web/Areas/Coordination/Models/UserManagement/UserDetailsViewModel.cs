namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class UserDetailsViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? Canton { get; set; }
    public string? District { get; set; }
    public string? FullAddress { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
