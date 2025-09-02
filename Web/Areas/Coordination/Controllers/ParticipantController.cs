using System.ComponentModel.DataAnnotations;
using LinaSys.Auth.Domain.AggregatesModel.User;
using LinaSys.BusinessIncubator.Application.Participants.Queries.ListProjectParticipants;
using LinaSys.BusinessIncubator.Application.Participants.Services;
using LinaSys.BusinessIncubator.Application.Project.Queries.GetProjectWithParticipants;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Orchestration.Application.Participants.Commands.BulkInviteParticipants;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Coordination.Controllers;

[Area("Coordination")]
[Authorize(Roles = $"{Roles.Coordinator},{Roles.Administrator},{Roles.GlobalAdministrator}")]
public class ParticipantController(
    ILogger<ParticipantController> logger,
    MediatorExecutor mediator,
    IDashboardBuilderService dashboardBuilder,
    UserManager<User> userManager) : DashboardBaseController(logger, mediator, dashboardBuilder)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            var query = new GetProjectWithParticipantsQuery(context.ProjectId!.Value);
            var result = await Mediator.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                this.MapErrorsToModelStateAndSetErrorToast<GetProjectWithParticipantsQuery>(result);
                return RedirectToAction("Index", "Dashboard");
            }

            var project = result.Value!;
            var viewModel = new CoordinatorParticipantManagementViewModel
            {
                ProjectId = context.ProjectId!.Value,
                ProjectName = project.Name,
                IncubatorId = context.IncubatorId!.Value,
                IncubatorName = context.IncubatorName ?? string.Empty,
            };

            return View(viewModel);
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
            Logger.LogError(ex, "Error loading participant management view for user {UserId}", CurrentUserId);
            this.SetErrorToast("Error al cargar la gestión de participantes.");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            Logger.LogInformation("Loading participants for project {ProjectId}", context.ProjectId!.Value);

            var query = new ListProjectParticipantsQuery(
                ProjectId: context.ProjectId!.Value,
                Start: request.Start,
                Length: request.Length,
                SearchValue: request.GlobalSearch,
                RoleFilter: Roles.Starter,  // Always filter for Starter role only
                StatusFilter: null);

            var result = await Mediator.SendOrThrowAsync(query);

            Logger.LogInformation("Found {TotalRecords} total participants, {FilteredRecords} filtered, returning {Count} for current page",
                result.TotalRecords, result.FilteredRecords, result.Participants.Count);

            var response = new DataTableResponse<ParticipantItem>
            {
                Draw = request.Draw,
                RecordsTotal = result.TotalRecords,
                RecordsFiltered = result.FilteredRecords,
                Data = result.Participants
            };

            return Json(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading participants list for user {UserId}", CurrentUserId);
            return Json(new DataTableResponse<object>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = []
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> BulkInvite()
    {
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            var query = new GetProjectWithParticipantsQuery(context.ProjectId!.Value);
            var result = await Mediator.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                this.MapErrorsToModelStateAndSetErrorToast<GetProjectWithParticipantsQuery>(result);
                return RedirectToAction("Index");
            }

            var project = result.Value!;
            var viewModel = new CoordinatorBulkInviteViewModel
            {
                ProjectId = context.ProjectId!.Value,
                ProjectName = project.Name,
                IncubatorId = context.IncubatorId!.Value,
                IncubatorName = context.IncubatorName ?? string.Empty,
                SendInvitationEmails = true
            };

            return View(viewModel);
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
            Logger.LogError(ex, "Error loading bulk invite view for user {UserId}", CurrentUserId);
            this.SetErrorToast("Error al cargar la invitación masiva.");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> BulkInvite(CoordinatorBulkInviteViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var context = DemandCurrentUserContext(requireProject: true);

            var command = new BulkInviteParticipantsCommand(
                ProjectId: context.ProjectId!.Value,
                FileStream: viewModel.InvitationFile!.OpenReadStream(),
                FileName: viewModel.InvitationFile.FileName,
                SendInvitationEmails: viewModel.SendInvitationEmails,
                CoordinatorUserId: CurrentUserId);

            var bulkResult = await Mediator.SendOrThrowAsync(command);

            // Check for validation errors (file format, data validation)
            if (bulkResult.ValidationErrors?.Any() == true)
            {
                // Show first 5 validation errors
                foreach (var error in bulkResult.ValidationErrors!.Take(5))
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(viewModel);
            }

            // Check for processing errors (runtime errors)
            if (bulkResult.ProcessingErrors?.Any() == true)
            {
                // Show first 5 processing errors
                foreach (var error in bulkResult.ProcessingErrors!.Take(5))
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                // If all invitations failed, return to view with errors
                if (bulkResult.FailedInvitations == bulkResult.TotalProcessed)
                {
                    this.SetErrorToast($"Error al procesar las invitaciones. {bulkResult.ProcessingErrors.FirstOrDefault()}");
                    return View(viewModel);
                }

                // If some succeeded, show warning but continue
                this.SetWarnToast($"Se procesaron {bulkResult.SuccessfulInvitations} de {bulkResult.TotalProcessed} invitaciones. Hubo {bulkResult.FailedInvitations} errores.");
                return RedirectToAction("Index");
            }

            this.SetSuccessToast($"Se procesaron {bulkResult.SuccessfulInvitations} invitaciones de {bulkResult.TotalProcessed} registros.");
            return RedirectToAction("Index");
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
            Logger.LogError(ex, "Error processing bulk invite for user {UserId}", CurrentUserId);
            this.SetErrorToast("Error al procesar las invitaciones masivas.");
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        try
        {
            var fileName = "plantilla_invitaciones_participantes.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var templateContent = ParticipantExcelService.GenerateInvitationTemplate();
            return File(templateContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating template for user {UserId}", CurrentUserId);
            this.SetErrorToast("Error al generar la plantilla.");
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            var query = new GetProjectWithParticipantsQuery(context.ProjectId!.Value);
            var result = await Mediator.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = "Error al obtener el proyecto." });
            }

            var project = result.Value!;
            // Statistics are already calculated in the DTO
            var totalParticipants = project.TotalParticipants;
            var activeParticipants = project.ActiveParticipants;

            // For form statistics, we need to query form submissions
            // TODO: Implement proper form status queries when form submission system is integrated
            var completedForms = 0; // Placeholder
            var pendingForms = totalParticipants - completedForms; // Placeholder logic

            var stats = new
            {
                totalParticipants,
                activeParticipants,
                completedForms,
                pendingForms
            };

            return Json(new { success = true, data = stats });
        }
        catch (UnauthorizedAccessException)
        {
            return Json(new { success = false, message = "Debe iniciar sesión para acceder a esta funcionalidad." });
        }
        catch (InvalidOperationException)
        {
            return Json(new { success = false, message = "Debe seleccionar un proyecto para ver las estadísticas." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading participant stats for user {UserId}", CurrentUserId);
            return Json(new { success = false, message = "Error al cargar las estadísticas." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        try
        {
            var context = DemandCurrentUserContext(requireProject: true);

            var query = new GetProjectWithParticipantsQuery(context.ProjectId!.Value);
            var result = await Mediator.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                this.MapErrorsToModelStateAndSetErrorToast<GetProjectWithParticipantsQuery>(result);
                return RedirectToAction("Index");
            }

            var project = result.Value!;
            // Convert project users to export data (Starter role only)
            var participantData = new List<ParticipantExportData>();
            foreach (var projectUser in project.StarterUsers)
            {
                var user = await userManager.FindByIdAsync(projectUser.UserId);
                participantData.Add(new ParticipantExportData
                {
                    Email = user?.Email ?? string.Empty,
                    FullName = $"{user?.Email?.Split('@')[0] ?? "Usuario"} - {user?.PhoneNumber ?? "Sin teléfono"}",
                    Role = projectUser.Role,
                    IsActive = projectUser.IsActive,
                    FormStatus = "Pendiente", // TODO: Get actual form status when integrated
                    JoinedAt = projectUser.JoinedAt,
                    LastActivity = projectUser.UpdatedAt
                });
            }

            var fileName = $"participantes_{project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var excelContent = ParticipantExcelService.ExportParticipants(participantData);
            return File(excelContent, contentType, fileName);
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
            Logger.LogError(ex, "Error exporting participants for user {UserId}", CurrentUserId);
            this.SetErrorToast("Error al exportar los participantes.");
            return RedirectToAction("Index");
        }
    }

    protected override string GetUserRole() => Roles.Coordinator;
}

public class CoordinatorParticipantManagementViewModel
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator ID.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;
}

/// <summary>
/// View model for coordinator bulk invitation.
/// </summary>
public class CoordinatorBulkInviteViewModel
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incubator ID.
    /// </summary>
    public long IncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the incubator name.
    /// </summary>
    public string IncubatorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSV/Excel file for bulk invitations.
    /// </summary>
    [Required(ErrorMessage = "El archivo de invitaciones es requerido.")]
    [Display(Name = "Archivo de invitaciones")]
    public IFormFile? InvitationFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send invitation emails.
    /// </summary>
    [Display(Name = "Enviar emails de invitación")]
    public bool SendInvitationEmails { get; set; } = true;
}
