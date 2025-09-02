using System.Diagnostics;
using LinaSys.BusinessIncubator.Application.Starter.Commands.CompleteTask;
using LinaSys.BusinessIncubator.Application.Starter.Queries.GetStarterDashboard;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Core.Application.Dashboard.Commands.MarkNotificationRead;
using LinaSys.Core.Application.Dashboard.Commands.UpdatePreferences;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Controllers;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

[Area("BusinessIncubators")]
[Authorize(Roles = Roles.Starter)]
public class StarterDashboardController(
    ILogger<StarterDashboardController> logger,
    MediatorExecutor mediator,
    IDashboardBuilderService dashboardBuilder,
    IBusinessIncubatorRepository repository,
    IDashboardAuditService auditService) : DashboardBaseController(logger, mediator, dashboardBuilder)
{
    [HttpGet]
    public async Task<IActionResult> Index(long? projectId)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var userId = CurrentUserId;

            // If no projectId provided, get user's projects and redirect to the first one
            if (!projectId.HasValue)
            {
                var userProjects = await repository.GetProjectsByUserAsync(userId);
                if (!userProjects.Any())
                {
                    TempData["Warning"] = "No tiene proyectos asignados. Contacte al administrador.";
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                // Redirect to the first active project
                var firstProject = userProjects.FirstOrDefault(p => p.Status == BusinessIncubator.Domain.Enums.ProjectStatus.Active);
                if (firstProject is null)
                {
                    TempData["Warning"] = "No tiene proyectos activos. Contacte al administrador.";
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                return RedirectToAction("Index", new { projectId = firstProject.Id });
            }

            // Verify project exists and user has access
            var project = await repository.GetProjectByIdAsync(projectId.Value);
            if (project is null)
            {
                TempData["Error"] = "Proyecto no encontrado.";
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            // Verify user has access to this project
            var userProjectsList = await repository.GetProjectsByUserAsync(userId);
            if (!userProjectsList.Any(p => p.Id == projectId.Value))
            {
                TempData["Error"] = "No tiene acceso a este proyecto.";
                return RedirectToAction("Index");
            }

            // Get starter dashboard
            var query = new GetStarterDashboardQuery(userId, projectId.Value);
            var dashboard = await Mediator.SendOrThrowAsync(query);

            // Log dashboard access
            await auditService.LogDashboardAccessAsync(userId, projectId.Value, "Starter");

            // Set ViewBag for project context
            ViewBag.Title = "Dashboard - " + project.Name;
            ViewBag.ProjectId = projectId.Value;
            ViewBag.ProjectName = project.Name;
            ViewBag.UserProjects = userProjectsList;
            ViewBag.ShowProjectSelector = true;
            ViewBag.CurrentProjectName = project.Name;
            ViewBag.ShowRefreshButton = true;
            ViewBag.ShowSettingsButton = true;
            ViewBag.ProjectBadge = project.Key;
            ViewBag.Subtitle = "Panel de control para emprendedores";

            // Set breadcrumbs
            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Inicio", Url = Url.Action("Index", "Home", new { area = string.Empty }) },
                new() { Text = "Incubadoras", Url = Url.Action("Index", "Home", new { area = "BusinessIncubators" }) },
                new() { Text = project.Name, Url = null },
                new() { Text = "Mi Dashboard", IsActive = true }
            };

            stopwatch.Stop();
            await auditService.LogPerformanceMetricAsync(userId, "LoadStarterDashboard", stopwatch.ElapsedMilliseconds);

            return View(dashboard);
        }
        catch (UnauthorizedAccessException)
        {
            await auditService.LogSecurityEventAsync(
                CurrentUserId,
                "UnauthorizedDashboardAccess",
                $"Attempted to access starter dashboard without authorization",
                false);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await auditService.LogPerformanceMetricAsync(
                CurrentUserId,
                "LoadStarterDashboard",
                stopwatch.ElapsedMilliseconds,
                "Failed");

            return HandleDashboardError(ex, "Error al cargar el dashboard del emprendedor");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTask(long taskId, string? notes, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        var command = new CompleteTaskCommand(userId, taskId, notes);
        await Mediator.SendOrThrowAsync(command, cancellationToken);

        await auditService.LogTaskCompletionAsync(userId, taskId, notes);
        return Json(new { success = true, message = "Tarea completada exitosamente." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(long notificationId)
    {
        var userId = CurrentUserId;
        var command = new MarkNotificationReadCommand(userId, notificationId);
        await Mediator.SendOrThrowAsync(command);

        await auditService.LogNotificationInteractionAsync(userId, notificationId, "MarkAsRead");
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesCommand command, CancellationToken cancellationToken)
    {
        await Mediator.SendOrThrowAsync(command, cancellationToken);

        await auditService.LogPreferenceUpdateAsync(
                command.UserId,
                "DashboardPreferences",
                "Multiple",
                "Updated");

        return Json(new { success = true, message = "Preferencias actualizadas exitosamente." });
    }

    [HttpGet]
    public async Task<IActionResult> GetWidgetData(string widgetCode, long projectId)
    {
        try
        {
            var userId = CurrentUserId;
            var parameters = new Dictionary<string, object> { { "projectId", projectId } };
            var data = await DashboardBuilder.LoadWidgetDataAsync(widgetCode, userId, parameters);

            return Json(new { success = true, data });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading widget data");
            return Json(new { success = false, message = "Error al cargar datos" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProjects()
    {
        try
        {
            var userId = CurrentUserId;
            var projects = await repository.GetProjectsByUserAsync(userId);

            var projectList = projects
                .Where(p => p.Status == BusinessIncubator.Domain.Enums.ProjectStatus.Active)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    key = p.Key,
                    status = p.Status.ToString(),
                    startDate = p.CreatedAt.ToString("yyyy-MM-dd"), // Using CreatedAt since StartDate doesn't exist
                    endDate = string.Empty // EndDate doesn't exist in Project entity
                })
                .ToList();

            return Json(new { success = true, projects = projectList });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user projects");
            return Json(new { success = false, message = "Error al obtener proyectos" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchProject(long projectId)
    {
        try
        {
            var userId = CurrentUserId;

            // Verify user has access to this project
            var userProjects = await repository.GetProjectsByUserAsync(userId);
            if (!userProjects.Any(p => p.Id == projectId))
            {
                await auditService.LogSecurityEventAsync(
                    userId,
                    "UnauthorizedProjectSwitch",
                    $"Attempted to switch to project {projectId} without access",
                    false);
                return Json(new { success = false, message = "No tiene acceso a este proyecto" });
            }

            // Store the selected project in session or user preferences
            HttpContext.Session.SetInt32("CurrentProjectId", (int)projectId);

            await auditService.LogDashboardAccessAsync(userId, projectId, "ProjectSwitch");

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", new { projectId })
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error switching project");
            return Json(new { success = false, message = "Error al cambiar de proyecto" });
        }
    }

    #region Widget Data Endpoints

    [HttpGet]
    public async Task<IActionResult> GetProgressData(long projectId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId;

            // Get progress data from services
            var progressService = HttpContext.RequestServices.GetService<LinaSys.BusinessIncubator.Application.Starter.Services.IProgressCalculationService>();
            if (progressService is not null)
            {
                var metrics = await progressService.CalculateDetailedProgressAsync(userId, projectId);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        overallProgress = metrics.OverallProgress,
                        currentPhase = metrics.CurrentPhase,
                        tasksCompleted = metrics.TasksCompleted,
                        tasksTotal = metrics.TasksTotal,
                        phaseProgress = metrics.PhaseProgress,
                        estimatedCompletionDate = metrics.EstimatedCompletionDate?.ToString("yyyy-MM-dd"),
                        velocityRate = metrics.VelocityRate,
                        weeklyChange = 5 // Mock value for now
                    }
                });
            }

            // Fallback to dashboard data
            var dashboard = await Mediator.SendOrThrowAsync(new GetStarterDashboardQuery(userId, projectId), cancellationToken);
            return Json(new
            {
                success = true,
                data = new
                {
                    overallProgress = dashboard.Progress.OverallProgress,
                    currentPhase = dashboard.Progress.CurrentPhase,
                    tasksCompleted = dashboard.Progress.TasksCompleted,
                    tasksTotal = dashboard.Progress.TasksTotal
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting progress data");
            return Json(new { success = false, message = "Error al obtener datos de progreso" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksData(long projectId, string filter = "all")
    {
        try
        {
            var userId = CurrentUserId;

            var taskService = HttpContext.RequestServices.GetService<LinaSys.BusinessIncubator.Application.Starter.Services.ITaskGenerationService>();
            if (taskService is not null)
            {
                var tasks = await taskService.GetPendingTasksAsync(userId, projectId);

                // Apply filter
                if (filter != "all")
                {
                    tasks = filter switch
                    {
                        "pending" => tasks.Where(t => t.Status == BusinessIncubator.Domain.Aggregates.Starter.TaskStatus.Pending).ToList(),
                        "in_progress" => tasks.Where(t => t.Status == BusinessIncubator.Domain.Aggregates.Starter.TaskStatus.InProgress).ToList(),
                        "completed" => tasks.Where(t => t.Status == BusinessIncubator.Domain.Aggregates.Starter.TaskStatus.Completed).ToList(),
                        "overdue" => tasks.Where(t => t.DueDate < DateTime.UtcNow && t.Status != BusinessIncubator.Domain.Aggregates.Starter.TaskStatus.Completed).ToList(),
                        _ => tasks
                    };
                }

                return Json(new
                {
                    success = true,
                    data = tasks.Select(t => new
                    {
                        id = t.Id,
                        title = t.Title,
                        description = t.Description,
                        type = t.Type,
                        category = t.Category,
                        priority = t.Priority,
                        status = t.Status,
                        dueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                        // EstimatedDuration, ActionUrl, ActionText don't exist in StarterTask
                        // Using empty/null values for now
                        estimatedDuration = (TimeSpan?)null,
                        actionUrl = (string?)null,
                        actionText = (string?)null
                    })
                });
            }

            // Fallback to dashboard data
            var dashboard = await Mediator.SendOrThrowAsync(new GetStarterDashboardQuery(userId, projectId));
            return Json(new
            {
                success = true,
                data = dashboard.Tasks
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting tasks data");
            return Json(new { success = false, message = "Error al obtener tareas" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMilestonesData(long projectId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId;
            var dashboard = await Mediator.SendOrThrowAsync(new GetStarterDashboardQuery(userId, projectId), cancellationToken);

            return Json(new
            {
                success = true,
                data = dashboard.Milestones.Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    description = m.Description,
                    status = m.Status,
                    dueDate = m.TargetDate.ToString("yyyy-MM-dd"),
                    progress = m.Progress,
                    // Phase doesn't exist in MilestoneDto
                    phase = "Unknown"
                })
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting milestones data");
            return Json(new { success = false, message = "Error al obtener hitos" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationsData(long projectId, bool unreadOnly = false)
    {
        try
        {
            var userId = CurrentUserId;

            var notificationService = HttpContext.RequestServices.GetService<LinaSys.BusinessIncubator.Application.Starter.Services.IStarterNotificationService>();
            if (notificationService is not null)
            {
                var notifications = await notificationService.GetUserNotificationsAsync(userId);

                if (unreadOnly)
                {
                    notifications = notifications.Where(n => !n.IsRead).ToList();
                }

                return Json(new
                {
                    success = true,
                    data = notifications.Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        category = n.Category,
                        priority = n.Priority,
                        isRead = n.IsRead,
                        actionUrl = n.ActionUrl,
                        actionText = n.ActionText,
                        createdDate = n.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    }),
                    unreadCount = notifications.Count(n => !n.IsRead)
                });
            }

            // Fallback to dashboard data
            var dashboard = await Mediator.SendOrThrowAsync(new GetStarterDashboardQuery(userId, projectId));
            return Json(new
            {
                success = true,
                data = dashboard.RecentActivities,
                unreadCount = dashboard.RecentActivities.Count()
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting notifications data");
            return Json(new { success = false, message = "Error al obtener notificaciones" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> RefreshWidget(string widgetCode, long projectId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = CurrentUserId;

            // Route to specific widget data endpoint based on widget code
            return widgetCode switch
            {
                "progress" => await GetProgressData(projectId, cancellationToken),
                "tasks" => await GetTasksData(projectId),
                "milestones" => await GetMilestonesData(projectId, cancellationToken),
                "notifications" => await GetNotificationsData(projectId),
                "resources" => await GetResourcesData(projectId),
                _ => await GetWidgetData(widgetCode, projectId)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing widget {WidgetCode}", widgetCode);
            return Json(new { success = false, message = "Error al actualizar widget" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetResourcesData(long projectId, string? phase = null)
    {
        try
        {
            var userId = CurrentUserId;

            // Get resources from repository
            var starterRepo = HttpContext.RequestServices.GetService<IStarterRepository>();
            if (starterRepo is not null)
            {
                var resources = await starterRepo.GetResourcesAsync(projectId, phase);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        categories = resources.GroupBy(r => r.Category).Select(g => new
                        {
                            category = g.Key,
                            count = g.Count(),
                            resources = g.Select(r => new
                            {
                                id = r.Id,
                                title = r.Title,
                                description = r.Description,
                                type = r.ResourceType,
                                url = r.Url,
                                isRequired = r.IsRequired,
                                viewCount = r.ViewCount
                            })
                        }),
                        totalResources = resources.Count,
                        requiredResources = resources.Count(r => r.IsRequired),
                        recentlyViewed = resources.Where(r => r.LastViewedBy == userId)
                            .OrderByDescending(r => r.LastViewedDate)
                            .Take(5)
                            .Select(r => new { id = r.Id, title = r.Title })
                    }
                });
            }

            // Fallback response
            return Json(new
            {
                success = true,
                data = new
                {
                    categories = new[]
                    {
                        new { category = "guides", count = 5 },
                        new { category = "templates", count = 3 },
                        new { category = "videos", count = 4 }
                    },
                    totalResources = 12,
                    requiredResources = 6
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting resources data");
            return Json(new { success = false, message = "Error al obtener recursos" });
        }
    }

    #endregion

    protected override string GetUserRole() => Roles.Starter;
}
