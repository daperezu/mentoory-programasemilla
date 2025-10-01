using System.Security.Claims;
using LinaSys.BusinessIncubator.Application.Public.Commands;
using LinaSys.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Observer.Controllers;

/// <summary>
/// Controller for Observer role to interact with projects.
/// </summary>
[Area("Observer")]
[Authorize(Roles = "Observer")]
public class ProjectsController : Controller
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly MediatorExecutor _mediatorExecutor;
    private readonly IMediator _mediator;

    public ProjectsController(
        ILogger<ProjectsController> logger,
        MediatorExecutor mediatorExecutor,
        IMediator mediator)
    {
        _logger = logger;
        _mediatorExecutor = mediatorExecutor;
        _mediator = mediator;
    }

    /// <summary>
    /// Observer dashboard showing interested projects and nearby projects.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Dashboard()
    {
        ViewData["Title"] = "Mis Proyectos de Interés";
        return View();
    }

    /// <summary>
    /// Records authenticated user's interest in a project.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpPost]
    public async Task<IActionResult> RecordInterest([FromBody] RecordProjectInterestRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get authenticated user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Usuario no autenticado." });
        }

        // Get request metadata
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var referrer = Request.Headers["Referer"].ToString();

        var command = new RecordProjectInterestCommand(
            request.ProjectId,
            userId,
            null, // No session ID needed for authenticated users
            request.InterestType,
            request.ObserverLatitude,
            request.ObserverLongitude,
            userAgent,
            ipAddress,
            referrer);

        var result = await _mediatorExecutor.SendAndLogIfFailureAsync(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to record interest for user {UserId} on project {ProjectId}", userId, request.ProjectId);
            return BadRequest(new { error = "Error al registrar interés en el proyecto." });
        }

        _logger.LogInformation("User {UserId} expressed interest in project {ProjectId}", userId, request.ProjectId);
        return Json(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Removes user's interest in a project.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public IActionResult RemoveInterest([FromBody] RemoveInterestRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Usuario no autenticado." });
        }

        // TODO: Implement RemoveProjectInterestCommand
        _logger.LogInformation("User {UserId} removed interest in project {ProjectId}", userId, request.ProjectId);

        return Json(new { success = true, message = "Interés removido exitosamente." });
    }

    /// <summary>
    /// Gets user's interested projects.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetMyInterests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Usuario no autenticado." });
        }

        // TODO: Implement GetUserProjectInterestsQuery
        var interests = new List<object>
        {
            new { projectId = Guid.NewGuid(), projectName = "Proyecto Demo", interestedAt = DateTime.Now.AddDays(-5) }
        };

        return Json(interests);
    }
}

public class RecordProjectInterestRequest
{
    public Guid ProjectId { get; set; }
    public string InterestType { get; set; } = "Interest"; // Interest, Favorite, Following
    public decimal? ObserverLatitude { get; set; }
    public decimal? ObserverLongitude { get; set; }
}

public class RemoveInterestRequest
{
    public Guid ProjectId { get; set; }
}