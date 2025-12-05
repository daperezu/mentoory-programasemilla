using LinaSys.BusinessIncubator.Application.Public.Queries;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Public.Controllers;

/// <summary>
/// Controller for public project discovery and engagement.
/// </summary>
[Area("Public")]
[AllowAnonymous]
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
    /// Displays the public homepage with project discovery features.
    /// Now loads projects by default without requiring location.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Descubre Proyectos de Emprendimiento";

        // Load latest projects for initial display (time-based)
        var query = new GetLatestProjectsQuery(MaxResults: 10, IncludeStages: true);
        var result = await _mediatorExecutor.SendAndLogIfFailureAsync(query);

        if (result.IsSuccess && result.Value != null)
        {
            // Populate image URLs
            foreach (var project in result.Value.Projects)
            {
                if (string.IsNullOrWhiteSpace(project.HeroImageUrl))
                {
                    var encodedBlobId = !string.IsNullOrWhiteSpace(project.HeroImageBlobId)
                        ? Uri.EscapeDataString(project.HeroImageBlobId)
                        : "placeholder";
                    project.HeroImageUrl = $"/Public/Images/{encodedBlobId}?type=hero&text={Uri.EscapeDataString(project.Name ?? "Proyecto")}";
                }
            }

            ViewData["LatestProjects"] = result.Value;
        }
        else
        {
            ViewData["LatestProjects"] = new LatestProjectsDto { Projects = new List<LatestProjectDto>() };
        }

        return View();
    }

    /// <summary>
    /// Gets the latest projects sorted by start date (no location required).
    /// Used for the default homepage view.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> GetLatestProjects(int? maxResults)
    {
        var query = new GetLatestProjectsQuery(
            MaxResults: maxResults ?? 10,
            IncludeStages: true);

        var result = await _mediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = "Error al obtener los proyectos más recientes." });
        }

        // Populate image URLs
        var response = result.Value!;
        foreach (var project in response.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.HeroImageUrl))
            {
                var encodedBlobId = !string.IsNullOrWhiteSpace(project.HeroImageBlobId)
                    ? Uri.EscapeDataString(project.HeroImageBlobId)
                    : "placeholder";
                project.HeroImageUrl = $"/Public/Images/{encodedBlobId}?type=hero&text={Uri.EscapeDataString(project.Name ?? "Proyecto")}";
            }
        }

        return Json(response);
    }

    /// <summary>
    /// Gets nearby projects based on user location.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpPost]
    public async Task<IActionResult> GetNearbyProjects([FromBody] GetNearbyProjectsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var query = new GetNearbyProjectsQuery(
            request.Latitude,
            request.Longitude,
            request.RadiusKm ?? 15.0,
            request.MaxResults ?? 20);

        var result = await _mediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = "Error al buscar proyectos cercanos." });
        }

        // Populate image URLs for each project
        var response = result.Value!;

        // Use our image handler endpoint which handles fallbacks internally
        foreach (var project in response.Projects)
        {
            var encodedBlobId = !string.IsNullOrWhiteSpace(project.HeroImageBlobId)
                ? Uri.EscapeDataString(project.HeroImageBlobId)
                : "placeholder";

            // Build the image URL with appropriate parameters
            project.HeroImageUrl = $"/Public/Images/{encodedBlobId}?type=hero&text={Uri.EscapeDataString(project.Name ?? "Proyecto")}";
        }

        return Json(response);
    }

    /// <summary>
    /// Gets project details for public viewing.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetProjectDetailsQuery(id);
        var result = await _mediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess || result.Value == null)
        {
            this.SetErrorToast("El proyecto no fue encontrado o no está disponible.");
            return RedirectToAction(nameof(Index));
        }

        var project = result.Value;
        ViewData["Title"] = project.Name;

        return View(project);
    }
}

public class GetNearbyProjectsRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public int? MaxResults { get; set; }
}