using LinaSys.Core.Application.Audit.Queries;
using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Coordination.Controllers;

/// <summary>
/// Controller for viewing audit logs.
/// </summary>
[Area("Coordination")]
[Route("Coordination/[controller]/[action]")]
[Authorize(Roles = "Administrador")]
public class AuditController(
    IMediator mediator,
    IAuditLogRepository auditRepository,
    ILogger<AuditController> logger,
    MediatorExecutor mediatorExecutor,
    IApplicationUrlService applicationUrlService) : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{

    /// <summary>
    /// Lists audit logs with filtering and pagination.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? entityType = null,
        string? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = new GetAuditLogsQuery(
            entityType,
            null,
            userId,
            action,
            startDate,
            endDate,
            page,
            pageSize);

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Error al cargar los registros de auditoría");
            return View(new AuditLogViewModel());
        }

        // Get filter options
        var entityTypes = await auditRepository.GetDistinctEntityTypesAsync();
        var actions = await auditRepository.GetDistinctActionsAsync();

        var viewModel = new AuditLogViewModel
        {
            Logs = result.Value!.Items,
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages,
            EntityTypes = entityTypes.ToList(),
            Actions = actions.ToList(),
            CurrentFilters = new AuditFilterViewModel
            {
                EntityType = entityType,
                UserId = userId,
                Action = action,
                StartDate = startDate,
                EndDate = endDate
            }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Views audit logs for a specific entity.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> EntityHistory(string entityType, string entityId)
    {
        var logs = await auditRepository.GetByEntityAsync(entityType, entityId);

        ViewBag.EntityType = entityType;
        ViewBag.EntityId = entityId;

        return View(logs);
    }

    /// <summary>
    /// Views audit logs for a specific user.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> UserActivity(string userId)
    {
        var logs = await auditRepository.GetByUserAsync(userId);

        ViewBag.UserId = userId;

        return View(logs);
    }

    /// <summary>
    /// Exports audit logs to CSV.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Export(
        string? entityType = null,
        string? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = new GetAuditLogsQuery(
            entityType,
            null,
            userId,
            action,
            startDate,
            endDate,
            1,
            10000); // Max export size

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Error al exportar los registros de auditoría");
            return RedirectToAction(nameof(Index));
        }

        var csv = GenerateCsv(result.Value!.Items);
        var fileName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(csv, "text/csv", fileName);
    }

    private byte[] GenerateCsv(List<AuditLogDto> logs)
    {
        using var memoryStream = new System.IO.MemoryStream();
        using var writer = new System.IO.StreamWriter(memoryStream, System.Text.Encoding.UTF8);

        // Write header
        writer.WriteLine("ID,Fecha,Usuario,Acción,Tipo Entidad,ID Entidad,IP,Agente Usuario");

        // Write data
        foreach (var log in logs)
        {
            writer.WriteLine($"{log.Id},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserName ?? "Sistema"},{log.Action},{log.EntityType},{log.EntityId},{log.IpAddress ?? string.Empty},{log.UserAgent ?? string.Empty}");
        }

        writer.Flush();
        return memoryStream.ToArray();
    }
}

/// <summary>
/// View model for audit log listing.
/// </summary>
public class AuditLogViewModel
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<string> EntityTypes { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public AuditFilterViewModel CurrentFilters { get; set; } = new();
}

/// <summary>
/// View model for audit filters.
/// </summary>
public class AuditFilterViewModel
{
    public string? EntityType { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
