using System.Security.Claims;
using LinaSys.Core.Application.Navigation.Queries;
using LinaSys.Orchestration.Application.UserContext.Commands;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.ViewComponents;

public class NavigationMenuViewComponent(
    MediatorExecutor mediatorExecutor,
    ILogger<NavigationMenuViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("No user ID found in claims, returning empty menu");
                return View(new NavigationMenuDto(
                    [],
                    new NavigationContextDto(string.Empty, null, null, null)));
            }

            // Get user context for role and incubator/project info
            var contextResult = await mediatorExecutor.SendAndLogIfFailureAsync(
                new GetEnrichedUserContextCommand(userId));

            if (!contextResult.IsSuccess || contextResult.Value == null)
            {
                logger.LogWarning("Failed to get user context for navigation menu");
                return View(new NavigationMenuDto(
                    [],
                    new NavigationContextDto(userId, null, null, null)));
            }

            var userContext = contextResult.Value;

            // Get navigation menu
            var menuResult = await mediatorExecutor.SendAndLogIfFailureAsync(
                new GetUserNavigationMenuQuery(
                    userId,
                    userContext.Role,
                    userContext.IncubatorId,
                    userContext.ProjectId));

            if (!menuResult.IsSuccess || menuResult.Value == null)
            {
                logger.LogWarning("Failed to get navigation menu for user {UserId}", userId);
                return View(new NavigationMenuDto(
                    [],
                    new NavigationContextDto(userId, userContext.Role,
                        userContext.IncubatorId, userContext.ProjectId)));
            }

            return View(menuResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating navigation menu");
            return View(new NavigationMenuDto(
                [],
                new NavigationContextDto(string.Empty, null, null, null)));
        }
    }
}
