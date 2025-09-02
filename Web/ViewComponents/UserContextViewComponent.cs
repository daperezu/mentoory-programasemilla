using System.Security.Claims;
using LinaSys.Orchestration.Application.UserContext.Commands;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.ViewComponents;

public class UserContextViewComponent(
    MediatorExecutor mediatorExecutor,
    ILogger<UserContextViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("No user ID found in claims for context display");
                return View(new UserContextDisplayViewModel
                {
                    HasContext = false
                });
            }

            // Get enriched user context
            var contextResult = await mediatorExecutor.SendAndLogIfFailureAsync(
                new GetEnrichedUserContextCommand(userId));

            if (!contextResult.IsSuccess || contextResult.Value == null)
            {
                logger.LogWarning("Failed to get user context for display");
                return View(new UserContextDisplayViewModel
                {
                    HasContext = false
                });
            }

            var context = contextResult.Value;

            // Determine if user has complete context based on role
            var hasContext = !string.IsNullOrEmpty(context.Role);
            var needsIncubator = !IsGlobalAdminRole(context.Role);
            var needsProject = needsIncubator && !IsIncubatorAdminRole(context.Role);

            // Check if context is complete
            if (hasContext)
            {
                if (needsIncubator && !context.IncubatorId.HasValue)
                {
                    hasContext = false;
                }
                else if (needsProject && !context.ProjectId.HasValue)
                {
                    hasContext = false;
                }
            }

            return View(new UserContextDisplayViewModel
            {
                Role = GetRoleDisplayName(context.Role),
                IncubatorName = TruncateName(context.IncubatorName, 20),
                ProjectName = TruncateName(context.ProjectName, 20),
                HasContext = hasContext,
                IsGlobalAdmin = context.IsGlobalAdministrator,
                ShowIncubator = needsIncubator && context.IncubatorId.HasValue,
                ShowProject = needsProject && context.ProjectId.HasValue
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating user context display");
            return View(new UserContextDisplayViewModel
            {
                HasContext = false
            });
        }
    }

    private static bool IsGlobalAdminRole(string? role)
    {
        return role == Roles.GlobalAdministrator;
    }

    private static bool IsIncubatorAdminRole(string? role)
    {
        return role == Roles.Administrator;
    }

    private static string? GetRoleDisplayName(string? role)
    {
        return role switch
        {
            Roles.Starter => "Participante",
            Roles.Coordinator => "Coordinador",
            Roles.Mentor => "Mentor",
            Roles.Guide => "Guía",
            Roles.Facilitator => "Facilitador",
            Roles.Liaison => "Enlace",
            Roles.Administrator => "Administrador",
            Roles.GlobalAdministrator => "Admin Global",
            _ => role
        };
    }

    private static string? TruncateName(string? name, int maxLength)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (name.Length <= maxLength)
        {
            return name;
        }

        return name.Substring(0, maxLength - 3) + "...";
    }
}

public class UserContextDisplayViewModel
{
    public string? Role { get; set; }
    public string? IncubatorName { get; set; }
    public string? ProjectName { get; set; }
    public bool HasContext { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public bool ShowIncubator { get; set; }
    public bool ShowProject { get; set; }
}
