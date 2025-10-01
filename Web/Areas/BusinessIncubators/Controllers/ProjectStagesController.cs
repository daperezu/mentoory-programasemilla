using LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectStage;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Web.Areas.BusinessIncubators.Models.ProjectStages;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

[Area("BusinessIncubators")]
[Route("BusinessIncubators/{businessIncubatorId:guid}/Projects/{projectId:guid}/Stages")]
[Authorize(Roles = $"{Roles.Coordinator},{Roles.Administrator},{Roles.GlobalAdministrator}")]
public class ProjectStagesController(
    ILogger<ProjectStagesController> logger,
    MediatorExecutor mediator,
    IApplicationUrlService applicationUrlService,
    BusinessIncubator.Domain.Repositories.IBusinessIncubatorRepository businessIncubatorRepository)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid businessIncubatorId, Guid projectId)
    {
        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        // Get project with stages using the internal ID
        var projectWithStages = await businessIncubatorRepository.GetProjectWithStagesAsync(project.Id);
        if (projectWithStages is null)
        {
            return NotFound();
        }

        var businessIncubator = await businessIncubatorRepository.GetBusinessIncubatorByExternalIdAsync(businessIncubatorId);
        if (businessIncubator is null)
        {
            return NotFound();
        }

        var viewModel = new ProjectStagesViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            BusinessIncubatorName = businessIncubator.Name,
            ProjectId = projectId,
            ProjectName = projectWithStages.Name,
            Stages = projectWithStages.ProjectStages.Select(stage => new StageViewModel
            {
                Id = stage.Id,
                Type = stage.Type,
                Title = stage.Title,
                Description = stage.Description,
                StartDate = stage.StartDate,
                EndDate = stage.EndDate,
                IsActive = stage.IsActive,
            }).OrderBy(s => (int)s.Type).ToList(),
        };

        return View(viewModel);
    }

    [HttpGet("{stageType}/Edit")]
    public async Task<IActionResult> Edit(Guid businessIncubatorId, Guid projectId, string stageType)
    {
        if (!Enum.TryParse<ProjectStageType>(stageType, out var stageTypeEnum))
        {
            return NotFound();
        }

        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        var projectWithStages = await businessIncubatorRepository.GetProjectWithStagesAsync(project.Id);
        if (projectWithStages is null)
        {
            return NotFound();
        }

        var stage = projectWithStages.ProjectStages.FirstOrDefault(s => s.Type == stageTypeEnum);
        if (stage is null)
        {
            return NotFound();
        }

        var businessIncubator = await businessIncubatorRepository.GetBusinessIncubatorByExternalIdAsync(businessIncubatorId);
        if (businessIncubator is null)
        {
            return NotFound();
        }

        var viewModel = new EditStageViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            BusinessIncubatorName = businessIncubator.Name,
            ProjectId = projectId,
            ProjectName = projectWithStages.Name,
            StageId = stage.Id,
            Type = stage.Type,
            Title = stage.Title,
            Description = stage.Description,
            StartDate = stage.StartDate,
            EndDate = stage.EndDate,
            IsActive = stage.IsActive,
        };

        return View(viewModel);
    }

    [HttpPost("{stageType}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid businessIncubatorId,
        Guid projectId,
        string stageType,
        EditStageViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            if (!Enum.TryParse<ProjectStageType>(stageType, out var stageTypeEnum))
            {
                ModelState.AddModelError(string.Empty, "Tipo de etapa inválido.");
                return View(viewModel);
            }

            var command = new UpdateProjectStageCommand(
                ProjectExternalId: projectId,
                Type: stageTypeEnum,
                Title: viewModel.Title,
                Description: viewModel.Description,
                StartDate: viewModel.StartDate,
                EndDate: viewModel.EndDate,
                IsActive: viewModel.IsActive);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

            if (result.IsSuccess)
            {
                var message = viewModel.IsActive
                    ? "La etapa ha sido actualizada y activada. Se han enviado notificaciones a los participantes."
                    : "La etapa ha sido actualizada exitosamente.";
                this.SetSuccessToast(message);
                return RedirectToAction(nameof(Index), new { businessIncubatorId, projectId });
            }

            MapErrorsToModelStateAndSetErrorToast<UpdateProjectStageCommand>(result);
        }

        // Reload business incubator and project names if validation fails
        var businessIncubator = await businessIncubatorRepository.GetBusinessIncubatorByExternalIdAsync(businessIncubatorId);
        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(projectId);

        viewModel.BusinessIncubatorId = businessIncubatorId;
        viewModel.BusinessIncubatorName = businessIncubator?.Name ?? string.Empty;
        viewModel.ProjectId = projectId;
        viewModel.ProjectName = project?.Name ?? string.Empty;

        return View(viewModel);
    }

    [HttpPost("{stageType}/Activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid businessIncubatorId, Guid projectId, string stageType)
    {
        if (!Enum.TryParse<ProjectStageType>(stageType, out var stageTypeEnum))
        {
            this.SetErrorToast("Tipo de etapa inválido.");
            return RedirectToAction(nameof(Index), new { businessIncubatorId, projectId });
        }

        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        var projectWithStages = await businessIncubatorRepository.GetProjectWithStagesAsync(project.Id);
        if (projectWithStages is null)
        {
            return NotFound();
        }

        var stage = projectWithStages.ProjectStages.FirstOrDefault(s => s.Type == stageTypeEnum);
        if (stage is null)
        {
            return NotFound();
        }

        var command = new UpdateProjectStageCommand(
            ProjectExternalId: projectId,
            Type: stageTypeEnum,
            Title: stage.Title,
            Description: stage.Description,
            StartDate: stage.StartDate,
            EndDate: stage.EndDate,
            IsActive: true);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("La etapa ha sido activada. Se han enviado notificaciones a los participantes.");
        }
        else
        {
            this.SetErrorToast("Error al activar la etapa.");
        }

        return RedirectToAction(nameof(Index), new { businessIncubatorId, projectId });
    }

    [HttpPost("{stageType}/Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid businessIncubatorId, Guid projectId, string stageType)
    {
        if (!Enum.TryParse<ProjectStageType>(stageType, out var stageTypeEnum))
        {
            this.SetErrorToast("Tipo de etapa inválido.");
            return RedirectToAction(nameof(Index), new { businessIncubatorId, projectId });
        }

        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        var projectWithStages = await businessIncubatorRepository.GetProjectWithStagesAsync(project.Id);
        if (projectWithStages is null)
        {
            return NotFound();
        }

        var stage = projectWithStages.ProjectStages.FirstOrDefault(s => s.Type == stageTypeEnum);
        if (stage is null)
        {
            return NotFound();
        }

        var command = new UpdateProjectStageCommand(
            ProjectExternalId: projectId,
            Type: stageTypeEnum,
            Title: stage.Title,
            Description: stage.Description,
            StartDate: stage.StartDate,
            EndDate: stage.EndDate,
            IsActive: false);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("La etapa ha sido desactivada.");
        }
        else
        {
            this.SetErrorToast("Error al desactivar la etapa.");
        }

        return RedirectToAction(nameof(Index), new { businessIncubatorId, projectId });
    }
}
