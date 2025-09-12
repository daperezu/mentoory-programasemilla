using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.KnowledgeStructure.Application.Module.Commands;
using LinaSys.KnowledgeStructure.Application.Module.Queries;
using LinaSys.Orchestration.Application.KnowledgeStructure.Commands;
using LinaSys.Web.Areas.KnowledgeStructure.Models.Module;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Controllers;

[Area("KnowledgeStructure")]
public class ModulesController(ILogger<ModulesController> logger, MediatorExecutor mediator, IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListModulesQuery(
            Start: request.Start,
            Length: request.Length,
            GlobalSearch: request.GlobalSearch,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            KnowledgeStructureId: long.TryParse(request.ColumnSearches.GetValueOrDefault("knowledgeStructureId"), out var ksId) ? ksId : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateModuleViewModel
        {
            KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateModuleViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken);
            return View(model);
        }

        var command = new CreateModuleOrchestrationCommand(model.Name, model.KnowledgeStructureId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Módulo creado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<CreateModuleOrchestrationCommand>(result);
        model.KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var query = new GetModuleByIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el módulo.");
            return RedirectToAction(nameof(Index));
        }

        var model = new EditModuleViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Description = result.Value.Description,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditModuleViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateModuleCommand(model.Id, model.Name, model.Description);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Módulo actualizado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<UpdateModuleCommand>(result);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteModuleCommand(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Módulo eliminado exitosamente.");
        }
        else
        {
            this.SetErrorToast("No se pudo eliminar el módulo.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ManageKnowledgeStructures(long id, CancellationToken cancellationToken)
    {
        // Get module with its knowledge structures
        var query = new GetModuleKnowledgeStructuresQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el módulo.");
            return RedirectToAction(nameof(Index));
        }

        // Get all knowledge structures for selection
        var allKnowledgeStructures = await MediatorExecutor.SendOrThrowAsync(
            new GetAllKnowledgeStructuresQuery(),
            cancellationToken);

        var assignedIds = result.Value.KnowledgeStructures.Select(ks => ks.Id).ToHashSet();
        var availableKnowledgeStructures = allKnowledgeStructures
            .Where(ks => !assignedIds.Contains(ks.Id) && ks.IsActive)
            .Select(ks => new SelectListItem
            {
                Value = ks.Id.ToString(),
                Text = ks.Name,
            })
            .ToList();

        var model = new ManageModuleKnowledgeStructuresViewModel
        {
            ModuleId = result.Value.ModuleId,
            ModuleName = result.Value.ModuleName,
            AssignedKnowledgeStructures = result.Value.KnowledgeStructures,
            AvailableKnowledgeStructures = availableKnowledgeStructures,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToKnowledgeStructure(long moduleId, long knowledgeStructureId, CancellationToken cancellationToken)
    {
        var command = new LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands.AddModuleToKnowledgeStructureCommand(
            knowledgeStructureId,
            moduleId);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Módulo asignado exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al asignar el módulo.");
        }

        return RedirectToAction(nameof(ManageKnowledgeStructures), new { id = moduleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromKnowledgeStructure(long moduleId, long structureModuleId, long knowledgeStructureId, CancellationToken cancellationToken)
    {
        var command = new LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands.RemoveModuleFromKnowledgeStructureCommand(
            knowledgeStructureId,
            structureModuleId);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Módulo removido exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al remover el módulo.");
        }

        return RedirectToAction(nameof(ManageKnowledgeStructures), new { id = moduleId });
    }

    private async Task<List<SelectListItem>> GetKnowledgeStructureOptionsAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllKnowledgeStructuresQuery();
        var result = await MediatorExecutor.SendOrThrowAsync(query, cancellationToken);

        return result
            .Select(ks => new SelectListItem
            {
                Value = ks.Id.ToString(),
                Text = ks.Name,
            })
            .ToList();
    }
}
