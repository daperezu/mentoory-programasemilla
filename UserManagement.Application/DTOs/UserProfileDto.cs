namespace LinaSys.UserManagement.Application.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, string> Preferences { get; set; } = new();
}

public class LocationDto
{
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? Canton { get; set; }
    public string? District { get; set; }
    public string? FullAddress { get; set; }
}