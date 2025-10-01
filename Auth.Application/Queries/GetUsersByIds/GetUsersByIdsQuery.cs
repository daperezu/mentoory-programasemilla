using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Auth.Application.Queries.GetUsersByIds;

/// <summary>
/// Query to batch load users by their IDs to avoid N+1 queries.
/// </summary>
public record GetUsersByIdsQuery(
    IEnumerable<string> UserIds) : IBaseRequest<Dictionary<string, UserBasicInfoDto>>;

/// <summary>
/// Basic user information DTO.
/// </summary>
public class UserBasicInfoDto
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
