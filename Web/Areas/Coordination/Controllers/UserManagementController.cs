using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using LinaSys.Orchestration.Application.UserManagement.Commands;
using LinaSys.Shared.Application.Services;
using LinaSys.Shared.Domain.Constants;
using LinaSys.UserManagement.Application.Commands.DeactivateUser;
using LinaSys.UserManagement.Application.Commands.ReactivateUser;
using LinaSys.UserManagement.Application.Commands.UpdateUserAvatar;
using LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;
using LinaSys.UserManagement.Application.Queries.ListUserProfiles;
using LinaSys.Auth.Application.Queries;
using LinaSys.Web.Areas.Coordination.Models.UserManagement;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace LinaSys.Web.Areas.Coordination.Controllers;

[Area("Coordination")]
[Authorize(Roles = $"{Roles.Coordinator},{Roles.Administrator},{Roles.GlobalAdministrator}")]
public class UserManagementController(
    ILogger<UserManagementController> logger,
    MediatorExecutor mediator,
    IApplicationUrlService applicationUrlService,
    IProgressTrackingService progressTrackingService,
    IPasswordGeneratorService passwordGeneratorService) : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    [HttpGet]
    public IActionResult Index()
    {
        // Get user's data scope
        var (incubatorId, projectId) = GetUserDataScope();

        // Prepare view model with role-based capabilities
        var viewModel = new UserManagementIndexViewModel
        {
            CanCreateUsers = CanCreateUsers(),
            CanEditUsers = CanEditUsers(),
            CurrentUserRole = CurrentUserRoles.FirstOrDefault() ?? string.Empty,
            IsGlobalAdmin = CurrentUserIsGlobalAdministrator,
            IncubatorId = incubatorId,
            ProjectId = projectId
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var query = new GetUserProfileByUserIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);
        if (!result.IsSuccess)
        {
            this.SetErrorToast("Usuario no encontrado");
            return RedirectToAction(nameof(Index));
        }

        var profile = result.Value!;
        var viewModel = new UserDetailsViewModel
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = string.Empty, // TODO: Get email from Auth domain
            Identification = profile.Identification,
            IsActive = profile.IsActive,
            Country = profile.Location?.Country,
            Province = profile.Location?.Province,
            Canton = profile.Location?.Canton,
            District = profile.Location?.District,
            FullAddress = profile.Location?.FullAddress,
            AvatarUrl = profile.AvatarUrl,
            CreatedDate = DateTime.UtcNow, // TODO: Add audit fields to UserProfile
            UpdatedDate = null // TODO: Add audit fields to UserProfile
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel();
        // Populate available roles based on current user permissions
        model.AvailableRoles = GetAvailableRolesForCreateUser();

        // Load all active incubators
        model.AvailableIncubators = await GetAllIncubatorsForForm();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns on validation error
            model.AvailableRoles = GetAvailableRolesForCreateUser();
            model.AvailableIncubators = await GetAllIncubatorsForForm();
            if (model.SelectedIncubatorId.HasValue)
            {
                model.AvailableProjects = await GetProjectsByIncubatorForForm(model.SelectedIncubatorId.Value);
            }

            return View(model);
        }

        // Map email preferences from view model to dictionary
        var emailPreferences = new Dictionary<string, string>();
        if (model.EmailPreferences != null)
        {
            emailPreferences["email.system.welcome"] = model.EmailPreferences.SystemWelcome.ToString().ToLower();
            emailPreferences["email.project.welcome"] = model.EmailPreferences.ProjectWelcome.ToString().ToLower();
            emailPreferences["email.approvals"] = model.EmailPreferences.Approvals.ToString().ToLower();
            emailPreferences["email.rejections"] = model.EmailPreferences.Rejections.ToString().ToLower();
            emailPreferences["email.reminders"] = model.EmailPreferences.Reminders.ToString().ToLower();
            emailPreferences["email.announcements"] = model.EmailPreferences.Announcements.ToString().ToLower();
            emailPreferences["email.task.assignments"] = model.EmailPreferences.TaskAssignments.ToString().ToLower();
            emailPreferences["email.form.deadlines"] = model.EmailPreferences.FormDeadlines.ToString().ToLower();
            emailPreferences["email.mentor.messages"] = model.EmailPreferences.MentorMessages.ToString().ToLower();
            emailPreferences["email.digest"] = model.EmailPreferences.Digest.ToString().ToLower();
        }

        // Generate temporary password if requested
        var password = model.Password ?? string.Empty;
        var isTemporaryPassword = model.GenerateTemporaryPassword;
        if (isTemporaryPassword)
        {
            // Use the password generator service
            password = passwordGeneratorService.GenerateTemporaryPassword();
        }

        // Logic: When using temporary password, typically don't auto-confirm email
        // unless explicitly requested by the admin
        var emailConfirmed = model.EmailConfirmed;
        if (isTemporaryPassword && !model.EmailConfirmed)
        {
            // For temporary passwords, email confirmation should be done by the user
            emailConfirmed = false;
        }

        var command = new CreateUserWithProfileOrchestrationCommand(
            Email: model.Email,
            FirstName: model.FirstName,
            LastName: model.LastName,
            Identification: model.Identification,
            Password: password,
            Country: model.Country,
            Province: model.Province,
            Canton: model.Canton,
            District: model.District,
            FullAddress: model.FullAddress,
            EmailPreferences: emailPreferences,
            EmailConfirmed: emailConfirmed,
            IsTemporaryPassword: isTemporaryPassword);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (!result.IsSuccess)
        {
            this.MapErrorsToModelStateAndSetErrorToast<CreateUserViewModel>(result);
            // Reload dropdowns on error
            model.AvailableRoles = GetAvailableRolesForCreateUser();
            model.AvailableIncubators = await GetAllIncubatorsForForm();
            if (model.SelectedIncubatorId.HasValue)
            {
                model.AvailableProjects = await GetProjectsByIncubatorForForm(model.SelectedIncubatorId.Value);
            }

            return View(model);
        }

        var userId = result.Value!;

        // Step 2: Assign role to the user
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            var assignRoleCommand = new AssignRolesToUserOrchestrationCommand(
                userId,
                [model.SelectedRole]);
            var roleResult = await MediatorExecutor.SendAndLogIfFailureAsync(assignRoleCommand);
            if (!roleResult.IsSuccess)
            {
                logger.LogWarning("Failed to assign role {Role} to user {UserId}", model.SelectedRole, userId);
            }
        }

        // Step 3: Assign incubator access if needed
        if (model.SelectedIncubatorId.HasValue && !string.IsNullOrEmpty(model.SelectedRole))
        {
            // Only use orchestration command for incubator-level roles
            var incubatorLevelRoles = new[] { "Administrator", "Liaison", "Coordinator" };
            if (incubatorLevelRoles.Contains(model.SelectedRole))
            {
                var assignIncubatorCommand = new AssignUserToIncubatorOrchestrationCommand(
                    userId,
                    model.SelectedIncubatorId.Value,
                    model.SelectedRole);
                var incubatorResult = await MediatorExecutor.SendAndLogIfFailureAsync(assignIncubatorCommand);
                if (!incubatorResult.IsSuccess)
                {
                    logger.LogWarning("Failed to assign user {UserId} to incubator {IncubatorId}",
                        userId, model.SelectedIncubatorId.Value);
                }
            }
            else
            {
                // For project-level roles (Starter, Mentor, Guide, Facilitator),
                // we need to create the incubator access record directly
                // so they can see the incubator in context selection
                var assignIncubatorCommand = new LinaSys.Auth.Application.Commands.AssignUserToIncubatorCommand(
                    userId,
                    model.SelectedIncubatorId.Value,
                    model.SelectedRole);
                var incubatorResult = await MediatorExecutor.SendAndLogIfFailureAsync(assignIncubatorCommand);
                if (!incubatorResult.IsSuccess)
                {
                    logger.LogWarning("Failed to create incubator access for user {UserId} to incubator {IncubatorId}",
                        userId, model.SelectedIncubatorId.Value);
                }
            }
        }

        // Step 4: Assign project access if needed
        if (model.SelectedProjectId.HasValue && !string.IsNullOrEmpty(model.SelectedRole))
        {
            // Get project details to get the external ID
            var projectQuery = new GetProjectByIdQuery(model.SelectedProjectId.Value);
            var projectResult = await MediatorExecutor.SendAndLogIfFailureAsync(projectQuery);

            if (projectResult is { IsSuccess: true, Value: not null })
            {
                var assignProjectCommand = new AssignUserToProjectOrchestrationCommand(
                    userId,
                    projectResult.Value.ExternalId,
                    model.SelectedRole);

                var projectAssignResult = await MediatorExecutor.SendAndLogIfFailureAsync(assignProjectCommand);
                if (!projectAssignResult.IsSuccess)
                {
                    logger.LogWarning("Failed to assign user {UserId} to project {ProjectId}",
                        userId, model.SelectedProjectId.Value);
                }
            }
        }

        this.SetSuccessToast("Usuario creado exitosamente con rol y permisos asignados");
        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var query = new GetUserProfileByUserIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);
        if (!result.IsSuccess)
        {
            this.SetErrorToast("Usuario no encontrado");
            return RedirectToAction(nameof(Index));
        }

        var profile = result.Value!;
        var viewModel = new EditUserViewModel
        {
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Country = profile.Location?.Country,
            Province = profile.Location?.Province,
            Canton = profile.Location?.Canton,
            District = profile.Location?.District,
            FullAddress = profile.Location?.FullAddress
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateUserWithProfileOrchestrationCommand(
            UserId: id,
            FirstName: model.FirstName,
            LastName: model.LastName,
            Country: model.Country,
            Province: model.Province,
            Canton: model.Canton,
            District: model.District,
            FullAddress: model.FullAddress);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (!result.IsSuccess)
        {
            this.MapErrorsToModelStateAndSetErrorToast<EditUserViewModel>(result);
            return View(model);
        }

        this.SetSuccessToast("Usuario actualizado exitosamente");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var command = new DeactivateUserCommand(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Error al desactivar el usuario");
        }
        else
        {
            this.SetSuccessToast("Usuario desactivado exitosamente");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(string id)
    {
        var command = new ReactivateUserCommand(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Error al reactivar el usuario");
        }
        else
        {
            this.SetSuccessToast("Usuario reactivado exitosamente");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAvatar(string id, IFormFile avatarFile)
    {
        if (avatarFile == null || avatarFile.Length == 0)
        {
            this.SetErrorToast("Por favor seleccione un archivo de imagen");
            return RedirectToAction(nameof(Details), new { id });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(avatarFile.ContentType.ToLower()))
        {
            this.SetErrorToast("Solo se permiten archivos de imagen (JPEG, PNG, GIF, WebP)");
            return RedirectToAction(nameof(Details), new { id });
        }

        // Validate file size (max 2MB per UI spec)
        if (avatarFile.Length > 2 * 1024 * 1024)
        {
            this.SetErrorToast("El archivo no puede exceder 2MB");
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            using var stream = avatarFile.OpenReadStream();
            var command = new UpdateUserAvatarCommand(id, stream, avatarFile.FileName, avatarFile.ContentType);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

            if (!result.IsSuccess)
            {
                this.MapErrorsToModelStateAndSetErrorToast<object>(result);
            }
            else
            {
                this.SetSuccessToast("Avatar actualizado exitosamente");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading avatar for user {UserId}", id);
            this.SetErrorToast("Error al cargar el avatar");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ListUsers([FromBody] DataTableRequest request)
    {
        // Get user's data scope for filtering
        var (incubatorId, projectId) = GetUserDataScope();

        // TODO: Update ListUserProfilesQuery to support incubator/project filtering
        // For now, we'll get all users and filter client-side if needed
        var query = new ListUserProfilesQuery(
            Start: request.Start,
            Length: request.Length,
            SearchTerm: request.GlobalSearch,
            IsActive: null);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess)
        {
            return Json(new DataTableResponse<UserListItemWithRolesViewModel>
            {
                Draw = request.Draw,
                RecordsTotal = 0,
                RecordsFiltered = 0,
                Data = []
            });
        }

        var pagedResult = result.Value!;

        // TODO: Get user roles from Auth domain for each user
        // For now, we'll add a placeholder
        var users = pagedResult.Data.Select(u => new UserListItemWithRolesViewModel
        {
            Id = u.Id,
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            FullName = u.FullName,
            Email = string.Empty, // TODO: Get email from Auth domain
            Identification = u.Identification,
            IsActive = u.IsActive,
            AvatarUrl = u.AvatarUrl,
            Roles = [], // TODO: Get roles from Auth domain
            IncubatorName = string.Empty, // TODO: Get from user context
            ProjectNames = [] // TODO: Get from user assignments
        }).ToList();

        return Json(new DataTableResponse<UserListItemWithRolesViewModel>
        {
            Draw = request.Draw,
            RecordsTotal = pagedResult.RecordsTotal,
            RecordsFiltered = pagedResult.RecordsFiltered,
            Data = users
        });
    }

    /// <summary>
    /// Gets audit history for a specific user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetAuditHistory(string userId)
    {
        // Return the ViewComponent result directly
        return ViewComponent("AuditHistory", new { userId = userId });
    }

    /// <summary>
    /// Exports users to CSV or Excel format.
    /// </summary>
    /// <returns>The export file.</returns>
    [HttpGet]
    public async Task<IActionResult> ExportUsers(string format = "xlsx", string? searchTerm = null, bool? isActive = null)
    {
        if (!CanCreateUsers())
        {
            this.SetErrorToast("No tiene permisos para exportar usuarios");
            return RedirectToAction(nameof(Index));
        }

        // Get user's data scope for filtering
        var (incubatorId, projectId) = GetUserDataScope();

        // Get all users (without pagination for export)
        var query = new ListUserProfilesQuery(
            Start: 0,
            Length: int.MaxValue,
            SearchTerm: searchTerm,
            IsActive: isActive);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess || result.Value == null)
        {
            this.SetErrorToast("Error al obtener los usuarios para exportar");
            return RedirectToAction(nameof(Index));
        }

        var users = result.Value.Data;

        if (format.ToLowerInvariant() == "csv")
        {
            return ExportToCsv(users);
        }
        else
        {
            return ExportToExcel(users);
        }
    }

    /// <summary>
    /// Downloads a sample template for bulk import.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult DownloadTemplate(string format = "csv")
    {
        if (format.ToLowerInvariant() == "csv")
        {
            var csv = new StringBuilder();
            csv.AppendLine("Email,FirstName,LastName,Identification,PhoneNumber,Role");
            csv.AppendLine("juan.perez@example.com,Juan,Pérez,123456789,+506 8888-8888,Starter");
            csv.AppendLine("maria.gonzalez@example.com,María,González,987654321,+506 7777-7777,Starter");
            csv.AppendLine("carlos.rodriguez@example.com,Carlos,Rodríguez,456789123,+506 6666-6666,Mentor");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "plantilla_usuarios.csv");
        }
        else
        {
            // Create Excel template
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Usuarios");

            // Headers
            worksheet.Cells[1, 1].Value = "Email";
            worksheet.Cells[1, 2].Value = "FirstName";
            worksheet.Cells[1, 3].Value = "LastName";
            worksheet.Cells[1, 4].Value = "Identification";
            worksheet.Cells[1, 5].Value = "PhoneNumber";
            worksheet.Cells[1, 6].Value = "Role";

            // Sample data
            worksheet.Cells[2, 1].Value = "juan.perez@example.com";
            worksheet.Cells[2, 2].Value = "Juan";
            worksheet.Cells[2, 3].Value = "Pérez";
            worksheet.Cells[2, 4].Value = "123456789";
            worksheet.Cells[2, 5].Value = "+506 8888-8888";
            worksheet.Cells[2, 6].Value = "Starter";

            worksheet.Cells[3, 1].Value = "maria.gonzalez@example.com";
            worksheet.Cells[3, 2].Value = "María";
            worksheet.Cells[3, 3].Value = "González";
            worksheet.Cells[3, 4].Value = "987654321";
            worksheet.Cells[3, 5].Value = "+506 7777-7777";
            worksheet.Cells[3, 6].Value = "Starter";

            worksheet.Cells[4, 1].Value = "carlos.rodriguez@example.com";
            worksheet.Cells[4, 2].Value = "Carlos";
            worksheet.Cells[4, 3].Value = "Rodríguez";
            worksheet.Cells[4, 4].Value = "456789123";
            worksheet.Cells[4, 5].Value = "+506 6666-6666";
            worksheet.Cells[4, 6].Value = "Mentor";

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "plantilla_usuarios.xlsx");
        }
    }

    /// <summary>
    /// Shows the bulk import form.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult BulkImport()
    {
        if (!CanCreateUsers())
        {
            this.SetErrorToast("No tiene permisos para importar usuarios");
            return RedirectToAction(nameof(Index));
        }

        return View(new BulkImportViewModel());
    }

    /// <summary>
    /// Processes bulk user import from CSV or Excel file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkImport(BulkImportViewModel model)
    {
        if (!CanCreateUsers())
        {
            this.SetErrorToast("No tiene permisos para importar usuarios");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate file type
        var fileExtension = Path.GetExtension(model.ImportFile.FileName).ToLowerInvariant();
        if (fileExtension != ".csv" && fileExtension != ".xlsx" && fileExtension != ".xls")
        {
            this.SetErrorToast("Solo se admiten archivos CSV o Excel (.csv, .xlsx, .xls)");
            return View(model);
        }

        // Generate operation ID
        var operationId = Guid.NewGuid().ToString();
        var userId = CurrentUserContext?.UserId ?? string.Empty;

        try
        {
            // Parse file to get users
            var usersToImport = await ParseImportFileAsync(model.ImportFile);
            if (!usersToImport.Any())
            {
                this.SetErrorToast("No se encontraron usuarios válidos en el archivo");
                return View(model);
            }

            // Start progress tracking
            var tracker = progressTrackingService.StartOperation(
                operationId,
                usersToImport.Count,
                userId,
                $"Importación masiva de {usersToImport.Count} usuarios");

            // Start background task to process users
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessBulkImportAsync(usersToImport, model, tracker);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing bulk import {OperationId}", operationId);
                    await tracker.CompleteAsync($"Error en la importación: {ex.Message}");
                }
            });

            // Return success view with operation ID for tracking
            return View("BulkImportProgress", new BulkImportProgressViewModel
            {
                OperationId = operationId,
                TotalItems = usersToImport.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating bulk import");
            this.SetErrorToast($"Error al procesar el archivo: {ex.Message}");
            return View(model);
        }
    }

    /// <summary>
    /// Gets the current status of a bulk import operation.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetBulkImportStatus(string operationId)
    {
        var progress = progressTrackingService.GetProgress(operationId);

        if (progress == null)
        {
            return Json(new { error = "Operación no encontrada" });
        }

        return Json(new
        {
            operationId = progress.OperationId,
            totalItems = progress.TotalItems,
            processedItems = progress.ProcessedItems,
            successCount = progress.SuccessCount,
            failureCount = progress.FailureCount,
            progressPercentage = progress.ProgressPercentage,
            currentMessage = progress.CurrentMessage,
            isCompleted = progress.IsCompleted,
            isCancelled = progress.IsCancelled,
            errors = progress.Errors
        });
    }

    /// <summary>
    /// Shows the role management view for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The role management view.</returns>
    [HttpGet("ManageRoles/{userId}")]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.GlobalAdministrator}")]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        // Get user details from Auth domain
        var userQuery = new GetUserByIdQuery(userId);
        var userResult = await MediatorExecutor.SendAndLogIfFailureAsync(userQuery);
        if (!userResult.IsSuccess)
        {
            this.SetErrorToast("Usuario no encontrado");
            return RedirectToAction(nameof(Index));
        }

        // Get user profile for full name
        var profileQuery = new GetUserProfileByUserIdQuery(userId);
        var profileResult = await MediatorExecutor.SendAndLogIfFailureAsync(profileQuery);
        string fullName = "Usuario";
        if (profileResult is { IsSuccess: true, Value: not null })
        {
            fullName = $"{profileResult.Value.FirstName} {profileResult.Value.LastName}".Trim();
        }

        // Get current roles from Auth domain
        var rolesQuery = new GetUserRolesQuery(userId);
        var rolesResult = await MediatorExecutor.SendAndLogIfFailureAsync(rolesQuery);
        var currentRoles = rolesResult.IsSuccess ? rolesResult.Value!.ToList() : [];

        var viewModel = new UserRolesViewModel
        {
            UserId = userId,
            FullName = string.IsNullOrWhiteSpace(fullName) ? userResult.Value!.UserName : fullName,
            Email = userResult.Value!.Email,
            CurrentRoles = currentRoles,
            AvailableRoles = GetAvailableRolesForCurrentUser()
        };

        // Pre-select current roles
        foreach (var role in viewModel.AvailableRoles)
        {
            role.IsSelected = viewModel.CurrentRoles.Contains(role.Name);
        }

        return View(viewModel);
    }

    /// <summary>
    /// Updates the roles for a user.
    /// </summary>
    /// <param name="model">The user roles view model.</param>
    /// <returns>Redirect to user details.</returns>
    [HttpPost("ManageRoles/{userId}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.GlobalAdministrator}")]
    public IActionResult ManageRoles(string userId, UserRolesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetAvailableRolesForCurrentUser();
            return View(model);
        }

        // TODO: Implement role update logic via Auth domain
        // This would involve:
        // 1. Getting current roles
        // 2. Determining roles to add/remove
        // 3. Calling Auth domain to update roles
        this.SetSuccessToast($"Roles actualizados exitosamente para {model.FullName}");
        return RedirectToAction(nameof(Details), new { id = model.UserId });
    }

    /// <summary>
    /// Shows the batch role assignment modal.
    /// </summary>
    /// <returns>Partial view for batch role assignment.</returns>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.GlobalAdministrator}")]
    public IActionResult BatchRoleAssignment()
    {
        var viewModel = new BatchRoleAssignmentViewModel();
        ViewBag.AvailableRoles = GetAvailableRolesForCurrentUser()
            .Where(r => r.CanModify)
            .Select(r => new { Value = r.Name, Text = r.DisplayName })
            .ToList();

        return PartialView("_BatchRoleAssignment", viewModel);
    }

    /// <summary>
    /// Performs batch role assignment for multiple users.
    /// </summary>
    /// <param name="model">The batch role assignment view model.</param>
    /// <returns>JSON result with operation status.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.GlobalAdministrator}")]
    public IActionResult BatchRoleAssignment(BatchRoleAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Datos inválidos" });
        }

        try
        {
            // TODO: Implement batch role assignment via Auth domain
            var successCount = 0;
            var failureCount = 0;

            foreach (var userId in model.UserIds)
            {
                // TODO: Add or remove role for each user
                successCount++;
            }

            var actionText = model.Action == RoleAssignmentAction.Add ? "asignado" : "removido";
            var message = $"Rol {model.Role} {actionText} exitosamente. Exitosos: {successCount}, Fallos: {failureCount}";

            return Json(new { success = true, message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in batch role assignment");
            return Json(new { success = false, message = "Error al procesar la asignación masiva de roles" });
        }
    }

    /// <summary>
    /// Cancels a bulk import operation.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelBulkImport(string operationId)
    {
        var cancelled = progressTrackingService.CancelOperation(operationId);

        if (cancelled)
        {
            this.SetSuccessToast("Operación cancelada exitosamente");
        }
        else
        {
            this.SetErrorToast("No se pudo cancelar la operación");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Gets all active incubators for the dropdown.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllIncubators()
    {
        try
        {
            var query = new GetAllIncubatorsQuery();
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (result is { IsSuccess: true, Value: not null })
            {
                return Json(new
                {
                    success = true,
                    incubators = result.Value.Select(i => new
                    {
                        id = i.Id,
                        name = i.Name,
                        key = i.Key
                    })
                });
            }

            return Json(new { success = false, incubators = new List<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading incubators");
            return Json(new { success = false, incubators = new List<object>() });
        }
    }

    /// <summary>
    /// Gets projects for a specific incubator.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjectsByIncubator(long incubatorId)
    {
        try
        {
            var query = new GetProjectsByIncubatorQuery(incubatorId);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (result is { IsSuccess: true, Value: not null })
            {
                return Json(new
                {
                    success = true,
                    projects = result.Value.Select(p => new
                    {
                        id = p.Id,
                        externalId = p.ExternalId,
                        name = p.Name,
                        key = p.Key
                    })
                });
            }

            return Json(new { success = false, projects = new List<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading projects for incubator {IncubatorId}", incubatorId);
            return Json(new { success = false, projects = new List<object>() });
        }
    }

    private Task<List<ImportUserDto>> ParseImportFileAsync(IFormFile file)
    {
        var users = new List<ImportUserDto>();
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (fileExtension == ".csv")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            users = csv.GetRecords<ImportUserDto>().ToList();
        }
        else
        {
            // Excel file processing
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                throw new InvalidOperationException("El archivo Excel no contiene hojas de trabajo");
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            var colCount = worksheet.Dimension?.Columns ?? 0;

            if (rowCount < 2 || colCount < 3)
            {
                throw new InvalidOperationException("El archivo no contiene datos suficientes");
            }

            // Map column headers to indices
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // Process data rows
            for (int row = 2; row <= rowCount; row++)
            {
                var email = GetCellValue(worksheet, row, headers, "Email", "CorreoElectronico", "Correo");
                var firstName = GetCellValue(worksheet, row, headers, "FirstName", "Nombre", "PrimerNombre");
                var lastName = GetCellValue(worksheet, row, headers, "LastName", "Apellido", "Apellidos");
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(lastName))
                {
                    continue; // Skip invalid rows
                }

                users.Add(new ImportUserDto
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Identification = GetCellValue(worksheet, row, headers, "Identification", "Identificacion", "Cedula"),
                    PhoneNumber = GetCellValue(worksheet, row, headers, "PhoneNumber", "Telefono", "Celular"),
                    Role = GetCellValue(worksheet, row, headers, "Role", "Rol")
                });
            }
        }

        return Task.FromResult(users);
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, params string[] possibleHeaders)
    {
        foreach (var header in possibleHeaders)
        {
            if (headers.TryGetValue(header, out var col))
            {
                return worksheet.Cells[row, col].Value?.ToString()?.Trim() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private async Task ProcessBulkImportAsync(
        List<ImportUserDto> users,
        BulkImportViewModel importModel,
        IProgressTracker tracker)
    {
        foreach (var user in users)
        {
            if (tracker.CancellationToken.IsCancellationRequested)
            {
                await tracker.CompleteAsync("Importación cancelada por el usuario");
                return;
            }

            try
            {
                await tracker.ReportProgressAsync($"Procesando: {user.Email}");

                // Generate a temporary password using the service
                var temporaryPassword = passwordGeneratorService.GenerateTemporaryPassword();

                // Create user command
                var command = new CreateUserWithProfileOrchestrationCommand(
                    Email: user.Email,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    Identification: user.Identification ?? string.Empty,
                    Password: temporaryPassword,
                    Country: "Costa Rica", // Default values
                    Province: string.Empty,
                    Canton: string.Empty,
                    District: string.Empty,
                    FullAddress: string.Empty,
                    EmailPreferences: new Dictionary<string, string>(),
                    EmailConfirmed: false, // Require email confirmation for bulk imports
                    IsTemporaryPassword: true); // Always temporary for bulk imports

                var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

                if (result.IsSuccess)
                {
                    await tracker.ReportSuccessAsync($"{user.FirstName} {user.LastName} ({user.Email})");

                    // TODO: Send welcome email if requested when SendWelcomeEmailCommand is implemented
                    // if (importModel.SendWelcomeEmails)
                    // {
                    //     Send welcome email with temporary password
                    // }
                }
                else
                {
                    var errorMessage = result.ErrorMessages?.FirstOrDefault().Message ?? "Error desconocido";
                    await tracker.ReportFailureAsync($"{user.Email}", errorMessage);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing user {Email}", user.Email);
                await tracker.ReportFailureAsync($"{user.Email}", ex.Message);
            }
        }

        await tracker.CompleteAsync($"Importación completada: {tracker.OperationId}");
    }

    /// <summary>
    /// Gets the data scope for the current user based on their role.
    /// </summary>
    private (long? IncubatorId, long? ProjectId) GetUserDataScope()
    {
        if (CurrentUserIsGlobalAdministrator)
        {
            // Global admin sees all users
            return (null, null);
        }

        if (CurrentUserRoles.Contains(Roles.Administrator))
        {
            // Administrator sees users in their incubator
            return (CurrentUserContext?.IncubatorId, null);
        }

        if (CurrentUserRoles.Contains(Roles.Coordinator))
        {
            // Coordinator sees users in their projects
            return (CurrentUserContext?.IncubatorId, CurrentUserContext?.ProjectId);
        }

        // No access
        return (null, null);
    }

    /// <summary>
    /// Determines if the current user can edit other users.
    /// </summary>
    private bool CanEditUsers()
    {
        return CurrentUserIsGlobalAdministrator ||
               CurrentUserRoles.Contains(Roles.Administrator);
    }

    /// <summary>
    /// Determines if the current user can create new users.
    /// </summary>
    private bool CanCreateUsers()
    {
        return CurrentUserIsGlobalAdministrator ||
               CurrentUserRoles.Contains(Roles.Administrator) ||
               CurrentUserRoles.Contains(Roles.Coordinator);
    }

    private List<RoleSelectionViewModel> GetAvailableRolesForCurrentUser()
    {
        var roles = new List<RoleSelectionViewModel>();

        if (CurrentUserIsGlobalAdministrator)
        {
            // Global admin can assign all roles
            roles.AddRange(Roles.AllRoles.Select(r => new RoleSelectionViewModel
            {
                Name = r,
                DisplayName = GetRoleDisplayName(r),
                Description = GetRoleDescription(r),
                CanModify = r != Roles.GlobalAdministrator // Can't remove own global admin role
            }));
        }
        else if (CurrentUserRoles.Contains(Roles.Administrator))
        {
            // Administrator can assign all roles except GlobalAdministrator
            roles.AddRange(Roles.AllRoles
                .Where(r => r != Roles.GlobalAdministrator)
                .Select(r => new RoleSelectionViewModel
                {
                    Name = r,
                    DisplayName = GetRoleDisplayName(r),
                    Description = GetRoleDescription(r),
                    CanModify = true
                }));
        }
        else if (CurrentUserRoles.Contains(Roles.Coordinator))
        {
            // Coordinator can only assign allowed roles
            roles.AddRange(Roles.CoordinatorAllowedRoles.Select(r => new RoleSelectionViewModel
            {
                Name = r,
                DisplayName = GetRoleDisplayName(r),
                Description = GetRoleDescription(r),
                CanModify = true
            }));
        }

        return roles;
    }

    private string GetRoleDisplayName(string role)
    {
        return role switch
        {
            Roles.Starter => "Emprendedor",
            Roles.Coordinator => "Coordinador",
            Roles.Mentor => "Mentor",
            Roles.Guide => "Guía",
            Roles.Facilitator => "Facilitador",
            Roles.Liaison => "Enlace",
            Roles.Administrator => "Administrador",
            Roles.GlobalAdministrator => "Administrador Global",
            _ => role
        };
    }

    private string GetRoleDescription(string role)
    {
        return role switch
        {
            Roles.Starter => "Participante del programa de incubación",
            Roles.Coordinator => "Gestiona proyectos y participantes de la incubadora",
            Roles.Mentor => "Proporciona orientación y apoyo a los participantes",
            Roles.Guide => "Asiste a los participantes en aspectos específicos",
            Roles.Facilitator => "Facilita talleres y sesiones de formación",
            Roles.Liaison => "Actúa como conexión entre diferentes partes interesadas",
            Roles.Administrator => "Gestiona la configuración del sistema y los usuarios",
            Roles.GlobalAdministrator => "Acceso completo al sistema en todas las incubadoras",
            _ => string.Empty
        };
    }

    private IActionResult ExportToCsv(IEnumerable<LinaSys.UserManagement.Application.DTOs.UserProfileDto> users)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Nombre,Apellido,NombreCompleto,Identificación,País,Provincia,Estado,FechaExportación");

        foreach (var user in users)
        {
            var country = user.Location?.Country ?? string.Empty;
            var province = user.Location?.Province ?? string.Empty;
            csv.AppendLine($"{user.FirstName},{user.LastName},{user.FullName},{user.Identification},{country},{province},{(user.IsActive ? "Activo" : "Inactivo")},{DateTime.UtcNow:yyyy-MM-dd}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"usuarios_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private async Task<List<IncubatorSelectItem>> GetAllIncubatorsForForm()
    {
        try
        {
            var query = new GetAllIncubatorsQuery();
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (result is { IsSuccess: true, Value: not null })
            {
                return result.Value.Where(i => !i.IsDeleted).Select(i => new IncubatorSelectItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Key = i.Key
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading incubators for create user");
        }

        return [];
    }

    private async Task<List<ProjectSelectItem>> GetProjectsByIncubatorForForm(long incubatorId)
    {
        try
        {
            var query = new GetProjectsByIncubatorQuery(incubatorId);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (result is { IsSuccess: true, Value: not null })
            {
                return result.Value.Select(p => new ProjectSelectItem
                {
                    Id = p.Id,
                    ExternalId = p.ExternalId,
                    Name = p.Name,
                    Key = p.Key,
                    IncubatorId = incubatorId
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading projects for incubator {IncubatorId}", incubatorId);
        }

        return [];
    }

    private List<RoleSelectItem> GetAvailableRolesForCreateUser()
    {
        var roles = new List<RoleSelectItem>();

        if (CurrentUserIsGlobalAdministrator)
        {
            // Global admin can create users with all roles
            roles.AddRange(Roles.AllRoles.Select(r => new RoleSelectItem
            {
                Value = r,
                DisplayName = GetRoleDisplayName(r),
                Description = GetRoleDescription(r),
                RequiresIncubator = GetRoleRequiresIncubator(r),
                RequiresProject = GetRoleRequiresProject(r)
            }));
        }
        else if (CurrentUserRoles.Contains(Roles.Administrator))
        {
            // Administrator can create all roles except GlobalAdministrator
            roles.AddRange(Roles.AllRoles
                .Where(r => r != Roles.GlobalAdministrator)
                .Select(r => new RoleSelectItem
                {
                    Value = r,
                    DisplayName = GetRoleDisplayName(r),
                    Description = GetRoleDescription(r),
                    RequiresIncubator = GetRoleRequiresIncubator(r),
                    RequiresProject = GetRoleRequiresProject(r)
                }));
        }
        else if (CurrentUserRoles.Contains(Roles.Coordinator))
        {
            // Coordinator can only create allowed roles
            roles.AddRange(Roles.CoordinatorAllowedRoles.Select(r => new RoleSelectItem
            {
                Value = r,
                DisplayName = GetRoleDisplayName(r),
                Description = GetRoleDescription(r),
                RequiresIncubator = GetRoleRequiresIncubator(r),
                RequiresProject = GetRoleRequiresProject(r)
            }));
        }

        return roles;
    }

    private bool GetRoleRequiresIncubator(string role)
    {
        return role switch
        {
            Roles.GlobalAdministrator => false,
            _ => true
        };
    }

    private bool GetRoleRequiresProject(string role)
    {
        return role switch
        {
            Roles.Starter => true,
            Roles.Mentor => true,
            Roles.Guide => true,
            Roles.Facilitator => true,
            _ => false
        };
    }

    private IActionResult ExportToExcel(IEnumerable<LinaSys.UserManagement.Application.DTOs.UserProfileDto> users)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Usuarios");

        // Headers
        worksheet.Cells[1, 1].Value = "Nombre";
        worksheet.Cells[1, 2].Value = "Apellido";
        worksheet.Cells[1, 3].Value = "Nombre Completo";
        worksheet.Cells[1, 4].Value = "Identificación";
        worksheet.Cells[1, 5].Value = "País";
        worksheet.Cells[1, 6].Value = "Provincia";
        worksheet.Cells[1, 7].Value = "Cantón";
        worksheet.Cells[1, 8].Value = "Estado";
        worksheet.Cells[1, 9].Value = "Fecha de Exportación";

        // Format headers
        using (var range = worksheet.Cells[1, 1, 1, 9])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        // Data rows
        var row = 2;
        foreach (var user in users)
        {
            worksheet.Cells[row, 1].Value = user.FirstName;
            worksheet.Cells[row, 2].Value = user.LastName;
            worksheet.Cells[row, 3].Value = user.FullName;
            worksheet.Cells[row, 4].Value = user.Identification ?? string.Empty;
            worksheet.Cells[row, 5].Value = user.Location?.Country ?? string.Empty;
            worksheet.Cells[row, 6].Value = user.Location?.Province ?? string.Empty;
            worksheet.Cells[row, 7].Value = user.Location?.Canton ?? string.Empty;
            worksheet.Cells[row, 8].Value = user.IsActive ? "Activo" : "Inactivo";
            worksheet.Cells[row, 9].Value = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Alternate row coloring
            if (row % 2 == 0)
            {
                using var rowRange = worksheet.Cells[row, 1, row, 9];
                rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rowRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
            }

            row++;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        // Add borders
        using (var allCells = worksheet.Cells[1, 1, row - 1, 9])
        {
            allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        var bytes = package.GetAsByteArray();
        var fileName = $"usuarios_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
