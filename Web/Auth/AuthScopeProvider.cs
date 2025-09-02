using System.Security.Claims;
using LinaSys.Shared.Application.Auth;

namespace LinaSys.Web.Auth;

/// <summary>
/// Implementation of IAuthScopeProvider using HttpContext for authorization scope.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthScopeProvider"/> class.
/// </remarks>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
public class AuthScopeProvider(IHttpContextAccessor httpContextAccessor) : IAuthScopeProvider
{

    /// <inheritdoc/>
    public string GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    /// <inheritdoc/>
    public List<string> GetCurrentUserRoles()
    {
        var roles = httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return roles ?? new List<string>();
    }

    /// <inheritdoc/>
    public bool IsAuthenticated()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <inheritdoc/>
    public string? GetCurrentUserEmail()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <inheritdoc/>
    public string? GetCurrentUserName()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <inheritdoc/>
    public bool HasClaim(string claimType, string claimValue)
    {
        return httpContextAccessor.HttpContext?.User?.HasClaim(claimType, claimValue) ?? false;
    }

    /// <inheritdoc/>
    public string? GetClaimValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    /// <inheritdoc/>
    public List<string> GetClaimValues(string claimType)
    {
        var values = httpContextAccessor.HttpContext?.User?.FindAll(claimType)
            .Select(c => c.Value)
            .ToList();

        return values ?? new List<string>();
    }
}
