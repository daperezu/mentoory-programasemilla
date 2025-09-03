using System.Security.Claims;
using LinaSys.Shared.Application.Auth;

namespace LinaSys.Web.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICurrentUserService"/> for accessing information about the current user.
/// </summary>
public class CurrentHttpUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    /// <inheritdoc />
    public string? UserId => accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <inheritdoc />
    public string? UserName => accessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
}
