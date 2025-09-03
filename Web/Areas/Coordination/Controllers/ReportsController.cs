using System.Diagnostics;
using LinaSys.BusinessIncubator.Application.Project.Queries.GetProjectWithParticipants;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.BusinessIncubator.Application.Reports.Commands.CreateReportTemplate;
using LinaSys.BusinessIncubator.Application.Reports.Commands.GenerateReport;
using LinaSys.BusinessIncubator.Application.Reports.Queries.ExportReport;
using LinaSys.BusinessIncubator.Application.Reports.Queries.GetReportTemplates;
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
[Authorize(Roles = $"{Roles.Coordinator},{Roles.Administrator},{Roles.GlobalAdministrator}")]
public class ReportsController(
    ILogger<ReportsController> logger,
    MediatorExecutor mediator,
    IDashboardBuilderService dashboardBuilder,
    IDashboardAuditService auditService,
    IMemoryCache cache) : DashboardBaseController(logger, mediator, dashboardBuilder)
{
    /// <summary>
    /// Create a new custom report template.
    /// </summary>
    /// <param name="name">Template name.</param>
    /// <param name="description">Template description.</param>
    /// <param name="reportType">Report type.</param>
    /// <param name="configurationJson">JSON configuration.</param>
    /// <returns>JSON with creation result.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTemplate(
        string name,
        string? description,
        string reportType,
        string configurationJson = "{}")
    {
        try
        {
            var contextResult = DemandCurrentUserContext(requireProject: true);

            var projectId = contextResult.ProjectId!.Value;

            // Parse report type
            if (!Enum.TryParse<BusinessIncubator.Domain.Enums.ReportType>(reportType, true, out var parsedReportType))
            {
                return Json(new
                { success = false, message = "Tipo de reporte no válido" });
            }

            // Create template
            var command = new CreateReportTemplateCommand(
                name,
                description,
                parsedReportType,
                false,
                projectId,
                configurationJson,
                CurrentUserId);
            var result = await Mediator.SendOrThrowAsync(command);

            // Log template creation
            await auditService.LogNotificationInteractionAsync(CurrentUserId, result!.TemplateId, "TemplateCreated");

            // Success message handled by client-side JavaScript
            return Json(new
            {
                success = true,
                data = result,
                message = "Plantilla creada exitosamente"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating report template");
            return Json(new
            { success = false, message = "Error al crear la plantilla de reporte" });
        }
    }

    /// <summary>
    /// Export a report in the specified format.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="format">Export format (Excel or CSV).</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <returns>File download or JSON with export result.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(
        long templateId,
        string format = "Excel",
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var contextResult = DemandCurrentUserContext(requireProject: true);

            var projectId = contextResult.ProjectId!.Value;

            // Parse export format
            if (!Enum.TryParse<ExportFormat>(format, true, out var exportFormat))
            {
                exportFormat = ExportFormat.Excel;
            }

            // Export report
            var query = new ExportReportQuery(
                templateId,
                projectId,
                startDate,
                endDate,
                exportFormat,
                null,
                CurrentUserId);
            var exportResult = await Mediator.SendOrThrowAsync(query);

            // Log export action
            await auditService.LogNotificationInteractionAsync(CurrentUserId, templateId, "ReportExported");

            // Return file download
            var fileBytes = Convert.FromBase64String(exportResult.FileContent);
            return File(fileBytes, exportResult.ContentType, exportResult.FileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting report for template {TemplateId}", templateId);
            return Json(new
            { success = false, message = "Error al exportar el reporte" });
        }
    }

    /// <summary>
    /// Generate a report from a template.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <returns>JSON with generation result.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(long templateId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var contextResult = DemandCurrentUserContext(requireProject: true);

            var projectId = contextResult.ProjectId!.Value;

            // Generate report
            var command = new GenerateReportCommand(templateId, projectId, startDate, endDate, null, CurrentUserId);
            var result = await Mediator.SendOrThrowAsync(command);

            // Log report generation
            await auditService.LogNotificationInteractionAsync(CurrentUserId, templateId, "ReportGenerated");

            return Json(new
            {
                success = true,
                data = result,
                message = "Reporte generado exitosamente"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating report for template {TemplateId}", templateId);
            return Json(new
            { success = false, message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Get report generation statistics for the dashboard.
    /// </summary>
    /// <returns>JSON with statistics.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var contextResult = DemandCurrentUserContext(requireProject: true);

            var projectId = contextResult.ProjectId!.Value;

            // Cache key for report stats
            var cacheKey = $"coordinator_report_stats_{projectId}";
            if (cache.TryGetValue<object>(cacheKey, out var cachedStats))
            {
                return Json(new
                { success = true, data = cachedStats });
            }

            // Get report templates count
            var templatesQuery = new GetReportTemplatesQuery(projectId, null, true, CurrentUserId);
            var templatesResult = await Mediator.SendOrThrowAsync(templatesQuery);

            var stats = new
            {
                TotalTemplates = templatesResult.TotalCount,
                TemplatesByType = templatesResult.Templates
                        .GroupBy(t => t.TypeDescription)
                        .ToDictionary(g => g.Key, g => g.Count()),
                LastGenerated = DateTime.Now.AddDays(-1), // Placeholder
                TotalGenerated = 42, // Placeholder
            };

            // Cache for 10 minutes
            cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return Json(new
            { success = true, data = stats });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting report statistics");
            return Json(new
            { success = false, message = "Error al obtener estadísticas de reportes" });
        }
    }

    /// <summary>
    /// Get available report templates for the current project.
    /// </summary>
    /// <param name="filterByType">Optional filter by report type.</param>
    /// <returns>JSON with report templates.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTemplates(string? filterByType = null)
    {
        try
        {
            var contextResult = DemandCurrentUserContext(requireProject: true);

            var projectId = contextResult.ProjectId!.Value;

            // Parse filter by type
            BusinessIncubator.Domain.Enums.ReportType? reportTypeFilter = null;
            if (!string.IsNullOrEmpty(filterByType) &&
                Enum.TryParse<BusinessIncubator.Domain.Enums.ReportType>(filterByType, true, out var parsedType))
            {
                reportTypeFilter = parsedType;
            }

            // Get report templates
            var query = new GetReportTemplatesQuery(projectId, reportTypeFilter, true, CurrentUserId);
            var result = await Mediator.SendOrThrowAsync(query);

            return Json(new
            { success = true, data = result });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting report templates");
            return Json(new
            { success = false, message = "Error al obtener plantillas de reportes" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            // Get project details using query
            var projectQuery = new GetProjectByIdQuery(context.ProjectId!.Value);
            var projectResult = await Mediator.SendAndLogIfFailureAsync(projectQuery);

            if (!projectResult.IsSuccess || projectResult.Value is null)
            {
                TempData["Error"] = "Proyecto no encontrado.";
                return RedirectToAction("Index", "ContextSelection", new { area = string.Empty });
            }

            var project = projectResult.Value;

            // Log reports access
            await auditService.LogDashboardAccessAsync(CurrentUserId, context.ProjectId.Value, "Reports");

            // Set ViewBag for context display
            ViewBag.Title = $"Reportes - {project.Name}";
            ViewBag.ProjectId = context.ProjectId.Value;
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectBadge = project.Key;
            ViewBag.IncubatorId = context.IncubatorId!.Value;
            ViewBag.IncubatorName = context.IncubatorName ?? "Incubadora";
            ViewBag.CurrentRole = context.Role;
            ViewBag.ShowContextSwitcher = true;
            ViewBag.ShowRefreshButton = true;
            ViewBag.Subtitle = "Generación y gestión de reportes";

            // Set breadcrumbs
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Inicio", Url = Url.Action("Index", "Home", new { area = string.Empty }) },
                new() { Text = "Coordinación", Url = null },
                new() { Text = context.IncubatorName ?? "Incubadora", Url = null },
                new() { Text = project.Name, Url = null },
                new() { Text = "Reportes", IsActive = true }
            };

            stopwatch.Stop();
            await auditService.LogPerformanceMetricAsync(CurrentUserId, "LoadReportsIndex", stopwatch.ElapsedMilliseconds);

            return View();
        }
        catch (UnauthorizedAccessException)
        {
            return new EmptyResult(); // Redirect already handled by DemandCurrentUserContext
        }
        catch (InvalidOperationException)
        {
            return new EmptyResult(); // Redirect already handled by DemandCurrentUserContext
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HandleDashboardError(ex, "Error al cargar la página de reportes");
        }
    }

    protected override string GetUserRole() => Roles.Coordinator;
}
