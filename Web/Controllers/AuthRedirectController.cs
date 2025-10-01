using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public class AuthRedirectController(ILogger<AuthRedirectController> logger, MediatorExecutor mediatorExecutor, IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{
    [HttpGet]
    public IActionResult RedirectToDashboard()
    {
        if (TryGetCurrentUserContext(out var userContext))
        {
            logger.LogInformation("Redirecting user {UserId} with role {Role}", CurrentUserId, userContext.Role);

            // Role-based redirection using role names
            return userContext.Role switch
            {
                Roles.Starter => RedirectToAction("Index", "Dashboard", new { area = "Participant" }),
                Roles.Mentor => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for Mentor
                Roles.Coordinator => RedirectToAction("Index", "Dashboard", new { area = "Coordination" }),
                Roles.Administrator => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for Administrator
                Roles.GlobalAdministrator => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for GlobalAdministrator
                Roles.Guide => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for Guide
                Roles.Facilitator => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for Facilitator
                Roles.Liaison => RedirectToAction("Index", "Home"), // TODO: Verify correct redirect path for Liaison
                _ => RedirectToAction("Index", "Home"),
            };
        }

        logger.LogWarning("User context not found for redirection");
        return RedirectToAction("Index", "ContextSelection");
    }
}
