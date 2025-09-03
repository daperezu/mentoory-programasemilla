namespace LinaSys.Web.Areas.Coordination.Models.UserManagement;

public class ListUserProfilesViewModel
{
    public List<UserListItemViewModel> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string? SearchTerm { get; set; }
}

public class UserListItemViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? AvatarUrl { get; set; }

    // TODO: Add these when audit fields are added to UserProfile
    // public DateTime CreatedDate { get; set; }
    // public DateTime? UpdatedDate { get; set; }
}
