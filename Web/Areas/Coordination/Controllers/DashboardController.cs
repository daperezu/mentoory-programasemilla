using System.Diagnostics;
using LinaSys.Auth.Application.Commands.Context;
using LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorDiagnosticStats;
using LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorParticipantStats;
using LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorPendingReviews;
using LinaSys.BusinessIncubator.Application.Dashboard.Queries.GetCoordinatorRecentActivity;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Core.Application.Dashboard.Commands.MarkNotificationRead;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Controllers;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinaSys.Web.Areas.Coordination.Controllers;

[Area("Coordination")]
[Authorize(Roles = $"{Roles.Coordinator},{Roles.Administrator}")]
public class DashboardController(
    ILogger<DashboardController> logger,
    MediatorExecutor mediatorExecutor,
    IDashboardBuilderService dashboardBuilder,
    IDashboardAuditService auditService,
    IMemoryCache cache) : DashboardBaseController(logger, mediatorExecutor, dashboardBuilder)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var userId = CurrentUserId;
            TryGetCurrentUserContext(out var context);

            // Get project details
            var project = await MediatorExecutor.SendOrThrowAsync(new GetProjectByIdQuery(context!.ProjectId!.Value));
            if (project is null)
            {
                TempData["Error"] = "Proyecto no encontrado.";
                return RedirectToAction("Index", "ContextSelection", new { area = string.Empty });
            }

            // Get incubator details
            var incubator = await MediatorExecutor.SendOrThrowAsync(new GetIncubatorByIdQuery(context!.IncubatorId!.Value));
            if (incubator is null)
            {
                TempData["Error"] = "Incubadora no encontrada.";
                return RedirectToAction("Index", "ContextSelection", new { area = string.Empty });
            }

            // Build coordinator dashboard using the service
            var dashboard = await DashboardBuilder.BuildCoordinatorDashboardAsync(userId);

            // Log dashboard access
            await auditService.LogDashboardAccessAsync(userId, context.ProjectId.Value, Roles.Coordinator);

            // Set ViewBag for context display
            ViewBag.Title = $"Dashboard Coordinador - {project.Name}";
            ViewBag.ProjectId = context.ProjectId.Value;
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectBadge = project.Key;
            ViewBag.IncubatorId = context.IncubatorId.Value;
            ViewBag.IncubatorName = incubator.Name;
            ViewBag.CurrentRole = Roles.Coordinator;
            ViewBag.ShowContextSwitcher = true;
            ViewBag.ShowRefreshButton = true;
            ViewBag.ShowSettingsButton = true;
            ViewBag.Subtitle = "Panel de control para coordinadores";

            // Set breadcrumbs
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Inicio", Url = Url.Action("Index", "Home", new { area = string.Empty }) },
                new() { Text = "Coordinación", Url = null },
                new() { Text = incubator.Name, Url = null },
                new() { Text = project.Name, Url = null },
                new() { Text = "Dashboard", IsActive = true }
            };

            stopwatch.Stop();
            await auditService.LogPerformanceMetricAsync(userId, "LoadCoordinatorDashboard", stopwatch.ElapsedMilliseconds);

            return View(dashboard);
        }
        catch (UnauthorizedAccessException)
        {
            await auditService.LogSecurityEventAsync(
                CurrentUserId,
                "UnauthorizedDashboardAccess",
                "Attempted to access coordinator dashboard without authorization",
                false);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await auditService.LogPerformanceMetricAsync(
                CurrentUserId,
                "LoadCoordinatorDashboard",
                stopwatch.ElapsedMilliseconds,
                "Failed");

            return HandleDashboardError(ex, "Error al cargar el dashboard del coordinador");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetParticipantStats()
    {
        TryGetCurrentUserContext(out var userContext);

        var projectId = userContext!.ProjectId!.Value;

        // Cache key for participant stats
        var cacheKey = $"coordinator_participant_stats_{projectId}";
        if (cache.TryGetValue<CoordinatorParticipantStatsDto>(cacheKey, out var cachedStats))
        {
            return Json(new { success = true, data = cachedStats });
        }

        // Get participant statistics using the query
        var query = new GetCoordinatorParticipantStatsQuery(projectId);
        var result = await Mediator.SendOrThrowAsync(query);

        // Cache for 5 minutes
        cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Json(new { success = true, data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetDiagnosticStats()
    {
        TryGetCurrentUserContext(out var userContext);

        var projectId = userContext!.ProjectId!.Value;

        // Cache key for diagnostic stats
        var cacheKey = $"coordinator_diagnostic_stats_{projectId}";
        if (cache.TryGetValue<CoordinatorDiagnosticStatsDto>(cacheKey, out var cachedStats))
        {
            return Json(new { success = true, data = cachedStats });
        }

        // Get diagnostic statistics using the query
        var query = new GetCoordinatorDiagnosticStatsQuery(projectId);
        var result = await Mediator.SendOrThrowAsync(query);

        // Cache for 5 minutes
        cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Json(new { success = true, data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingReviews(CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserContext(out var userContext))
        {
            // Get pending reviews using the query
            var query = new GetCoordinatorPendingReviewsQuery(userContext.ProjectId!.Value);
            var result = await Mediator.SendOrThrowAsync(query, cancellationToken);

            return Json(new { success = true, data = result });
        }

        return Json(new { success = false, message = "Contexto de usuario no válido" });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentActivity()
    {
        TryGetCurrentUserContext(out var userContext);

        // Get recent activity using the query
        var query = new GetCoordinatorRecentActivityQuery(userContext!.ProjectId!.Value);
        var result = await Mediator.SendOrThrowAsync(query);

        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(long notificationId, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationReadCommand(CurrentUserId, notificationId);
        await Mediator.SendOrThrowAsync(command, cancellationToken);

        await auditService.LogNotificationInteractionAsync(CurrentUserId, notificationId, "MarkAsRead");

        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> RefreshWidget(string widgetCode, CancellationToken cancellationToken)
    {
        try
        {
            return widgetCode switch
            {
                "participants" => await GetParticipantStats(),
                "diagnostics" => await GetDiagnosticStats(),
                "reviews" => await GetPendingReviews(cancellationToken),
                "activity" => await GetRecentActivity(),
                _ => Json(new { success = false, message = "Widget no reconocido" })
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing widget {WidgetCode}", widgetCode);
            return Json(new { success = false, message = "Error al actualizar widget" });
        }
    }

    protected override string GetUserRole() => "coordinator";
}
