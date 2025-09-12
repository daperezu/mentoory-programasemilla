using LinaSys.Core.Application.Dashboard.Queries.GetDashboard;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Application.Services;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public abstract class DashboardBaseController(
    ILogger<DashboardBaseController> logger,
    MediatorExecutor mediator,
    IApplicationUrlService applicationUrlService,
    IDashboardBuilderService dashboardBuilder)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    protected MediatorExecutor Mediator { get; } = mediator;
    protected IDashboardBuilderService DashboardBuilder { get; } = dashboardBuilder;
    protected ILogger<DashboardBaseController> Logger { get; } = logger;

    protected async Task<DashboardDto> GetDashboardAsync()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or does not have a valid role.");
        }

        var role = GetUserRole();

        var query = new GetDashboardQuery(userId, role);
        return await Mediator.SendOrThrowAsync(query);
    }

    protected abstract string GetUserRole();

    protected IActionResult HandleDashboardError(Exception ex, string? customMessage = null)
    {
        Logger.LogError(ex, customMessage ?? "Error al cargar el dashboard");

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { success = false, message = customMessage ?? "Error al cargar el dashboard" });
        }

        TempData["Error"] = customMessage ?? "Error al cargar el dashboard. Por favor, intente nuevamente.";
        return RedirectToAction("Index", "Home");
    }

    protected async Task<IActionResult> GetWidgetData(string widgetCode)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                Logger.LogWarning("User is not authenticated or does not have a valid role.");
                return Json(new { success = false, message = "Usuario no autenticado o sin rol válido" });
            }

            var data = await DashboardBuilder.LoadWidgetDataAsync(widgetCode, userId);
            return Json(new { success = true, data });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading widget data for {WidgetCode}", widgetCode);
            return Json(new { success = false, message = "Error al cargar datos del widget" });
        }
    }
}
