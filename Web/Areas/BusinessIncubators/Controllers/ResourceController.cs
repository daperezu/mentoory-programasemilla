using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.Auth;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

[Area("BusinessIncubators")]
[Authorize(Roles = "Starter,Mentor,Administrator")]
public class ResourceController(
    ILogger<ResourceController> logger,
    MediatorExecutor mediatorExecutor,
    IStarterRepository starterRepository,
    IBusinessIncubatorRepository businessRepository,
    ICurrentUserService currentUserService) : AuthorizedBaseController(logger, mediatorExecutor)
{
    [HttpGet]
    public async Task<IActionResult> Index(long projectId, string? phase = null, string? category = null)
    {
        try
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // Verify project access
            var project = await businessRepository.GetProjectByIdAsync(projectId);
            if (project is null)
            {
                this.SetErrorToast("Proyecto no encontrado");
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            // Get resources
            var resources = await starterRepository.GetResourcesAsync(projectId, phase);

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                resources = resources.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Group resources by category
            var groupedResources = resources
                .GroupBy(r => r.Category)
                .Select(g => new ResourceCategoryViewModel
                {
                    Category = g.Key,
                    DisplayName = GetCategoryDisplayName(g.Key),
                    Icon = GetCategoryIcon(g.Key),
                    Resources = g.Select(r => new ResourceViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        ResourceType = r.ResourceType,
                        Url = r.Url ?? string.Empty,
                        Phase = r.Phase,
                        IsRequired = r.IsRequired,
                        ViewCount = r.ViewCount,
                        TypeIcon = GetResourceTypeIcon(r.ResourceType),
                        TypeColor = GetResourceTypeColor(r.ResourceType)
                    }).ToList()
                })
                .OrderBy(g => g.Category)
                .ToList();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;
            ViewBag.CurrentPhase = phase ?? "all";
            ViewBag.CurrentCategory = category ?? "all";
            ViewBag.Title = "Recursos de Aprendizaje";

            // Set breadcrumbs
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Inicio", Url = Url.Action("Index", "Home", new { area = string.Empty }) },
                new() { Text = "Incubadoras", Url = Url.Action("Index", "Home", new { area = "BusinessIncubators" }) },
                new() { Text = project.Name, Url = Url.Action("Index", "StarterDashboard", new { projectId }) },
                new() { Text = "Recursos", IsActive = true }
            };

            var model = new ResourcesIndexViewModel
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Categories = groupedResources,
                TotalResources = resources.Count,
                RequiredResources = resources.Count(r => r.IsRequired),
                AvailablePhases = ["diagnosis", "development", "validation", "implementation", "growth"],
                AvailableCategories = resources.Select(r => r.Category).Distinct().ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading resources for project {ProjectId}", projectId);
            this.SetErrorToast("Error al cargar los recursos");
            return RedirectToAction("Index", "StarterDashboard", new { projectId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> View(long id, long projectId)
    {
        try
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // Get resource
            var resource = await starterRepository.GetResourceByIdAsync(id);
            if (resource is null)
            {
                this.SetErrorToast("Recurso no encontrado");
                return RedirectToAction("Index", new { projectId });
            }

            // Record view
            await starterRepository.RecordResourceViewAsync(id, userId);

            // Handle different resource types
            switch (resource.ResourceType.ToLower())
            {
                case "pdf":
                case "doc":
                case "excel":
                case "powerpoint":
                    if (!string.IsNullOrEmpty(resource.FilePath))
                    {
                        return File(resource.FilePath, GetMimeType(resource.ResourceType), resource.Title);
                    }

                    break;

                case "video":
                case "youtube":
                case "link":
                    if (!string.IsNullOrEmpty(resource.Url))
                    {
                        return Redirect(resource.Url);
                    }

                    break;

                case "template":
                    return RedirectToAction("Download", new { id, projectId });
            }

            // Default: show resource details
            var model = new ResourceDetailViewModel
            {
                Id = resource.Id,
                ProjectId = projectId,
                Title = resource.Title,
                Description = resource.Description,
                Category = resource.Category,
                ResourceType = resource.ResourceType,
                Url = resource.Url ?? string.Empty,
                Phase = resource.Phase,
                IsRequired = resource.IsRequired,
                ViewCount = resource.ViewCount + 1,
                LastViewedDate = DateTime.UtcNow
            };

            ViewBag.Title = resource.Title;
            ViewBag.ProjectId = projectId;

            return View("Details", model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error viewing resource {ResourceId}", id);
            this.SetErrorToast("Error al acceder al recurso");
            return RedirectToAction("Index", new { projectId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Download(long id, long projectId)
    {
        try
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // Get resource
            var resource = await starterRepository.GetResourceByIdAsync(id);
            if (resource is null)
            {
                this.SetErrorToast("Recurso no encontrado");
                return RedirectToAction("Index", new { projectId });
            }

            // Record view/download
            await starterRepository.RecordResourceViewAsync(id, userId);

            // Check if file exists
            if (string.IsNullOrEmpty(resource.FilePath))
            {
                this.SetErrorToast("Archivo no disponible");
                return RedirectToAction("Index", new { projectId });
            }

            var fileName = $"{resource.Title}.{GetFileExtension(resource.ResourceType)}";
            var mimeType = GetMimeType(resource.ResourceType);

            return File(resource.FilePath, mimeType, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading resource {ResourceId}", id);
            this.SetErrorToast("Error al descargar el recurso");
            return RedirectToAction("Index", new { projectId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(long projectId, string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index", new { projectId });
            }

            var resources = await starterRepository.GetResourcesAsync(projectId, null);

            // Filter by query
            var filteredResources = resources
                .Where(r => r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           r.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.SearchQuery = query;
            ViewBag.ProjectId = projectId;
            ViewBag.ResultCount = filteredResources.Count;

            var model = filteredResources.Select(r => new ResourceViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ResourceType = r.ResourceType,
                Category = r.Category,
                Phase = r.Phase,
                IsRequired = r.IsRequired,
                ViewCount = r.ViewCount,
                TypeIcon = GetResourceTypeIcon(r.ResourceType),
                TypeColor = GetResourceTypeColor(r.ResourceType)
            }).ToList();

            return View("SearchResults", model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching resources");
            this.SetErrorToast("Error al buscar recursos");
            return RedirectToAction("Index", new { projectId });
        }
    }

    #region Helper Methods

    private string GetCategoryDisplayName(string category)
    {
        return category switch
        {
            "guide" => "Guías",
            "template" => "Plantillas",
            "video" => "Videos",
            "article" => "Artículos",
            "tool" => "Herramientas",
            "document" => "Documentos",
            "course" => "Cursos",
            "example" => "Ejemplos",
            _ => category
        };
    }

    private string GetCategoryIcon(string category)
    {
        return category switch
        {
            "guide" => "fas fa-book",
            "template" => "fas fa-file-alt",
            "video" => "fas fa-video",
            "article" => "fas fa-newspaper",
            "tool" => "fas fa-tools",
            "document" => "fas fa-file",
            "course" => "fas fa-graduation-cap",
            "example" => "fas fa-lightbulb",
            _ => "fas fa-folder"
        };
    }

    private string GetResourceTypeIcon(string type)
    {
        return type?.ToLower() switch
        {
            "pdf" => "fas fa-file-pdf",
            "doc" => "fas fa-file-word",
            "excel" => "fas fa-file-excel",
            "powerpoint" => "fas fa-file-powerpoint",
            "video" => "fas fa-play-circle",
            "youtube" => "fab fa-youtube",
            "link" => "fas fa-external-link-alt",
            "template" => "fas fa-file-code",
            _ => "fas fa-file"
        };
    }

    private string GetResourceTypeColor(string type)
    {
        return type?.ToLower() switch
        {
            "pdf" => "danger",
            "doc" => "primary",
            "excel" => "success",
            "powerpoint" => "warning",
            "video" => "info",
            "youtube" => "danger",
            "link" => "secondary",
            "template" => "dark",
            _ => "secondary"
        };
    }

    private string GetMimeType(string resourceType)
    {
        return resourceType?.ToLower() switch
        {
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "excel" => "application/vnd.ms-excel",
            "powerpoint" => "application/vnd.ms-powerpoint",
            "template" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }

    private string GetFileExtension(string resourceType)
    {
        return resourceType?.ToLower() switch
        {
            "pdf" => "pdf",
            "doc" => "docx",
            "excel" => "xlsx",
            "powerpoint" => "pptx",
            "template" => "xlsx",
            _ => "bin"
        };
    }

    #endregion
}

#region ViewModels

public class ResourcesIndexViewModel
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; }
    public List<ResourceCategoryViewModel> Categories { get; set; } = [];
    public int TotalResources { get; set; }
    public int RequiredResources { get; set; }
    public string[] AvailablePhases { get; set; }
    public List<string> AvailableCategories { get; set; }
}

public class ResourceCategoryViewModel
{
    public string Category { get; set; }
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public List<ResourceViewModel> Resources { get; set; } = [];
}

public class ResourceViewModel
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ResourceType { get; set; }
    public string Category { get; set; }
    public string Url { get; set; }
    public string Phase { get; set; }
    public bool IsRequired { get; set; }
    public int ViewCount { get; set; }
    public string TypeIcon { get; set; }
    public string TypeColor { get; set; }
}

public class ResourceDetailViewModel
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string ResourceType { get; set; }
    public string Url { get; set; }
    public string Phase { get; set; }
    public bool IsRequired { get; set; }
    public int ViewCount { get; set; }
    public DateTime LastViewedDate { get; set; }
}

#endregion
