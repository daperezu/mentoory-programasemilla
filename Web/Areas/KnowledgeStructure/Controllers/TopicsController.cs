using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.KnowledgeStructure.Application.Module.Commands;
using LinaSys.KnowledgeStructure.Application.Module.Queries;
using LinaSys.KnowledgeStructure.Application.Topic.Commands;
using LinaSys.KnowledgeStructure.Application.Topic.Queries;
using LinaSys.Orchestration.Application.KnowledgeStructure.Commands;
using LinaSys.Web.Areas.KnowledgeStructure.Models.Topic;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Controllers;

[Area("KnowledgeStructure")]
public class TopicsController(ILogger<TopicsController> logger, MediatorExecutor mediator)
    : AuthorizedBaseController(logger, mediator)
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListTopicsQuery(
            Start: request.Start,
            Length: request.Length,
            GlobalSearch: request.GlobalSearch,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            StructureModuleId: long.TryParse(request.ColumnSearches.GetValueOrDefault("structureModuleId"), out var moduleId) ? moduleId : null,
            KnowledgeStructureId: long.TryParse(request.ColumnSearches.GetValueOrDefault("knowledgeStructureId"), out var ksId) ? ksId : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateTopicViewModel
        {
            KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTopicViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCreateViewModelOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateTopicOrchestrationCommand(model.Name, model.Description, model.StructureModuleId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema creado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<CreateTopicOrchestrationCommand>(result);
        await LoadCreateViewModelOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var query = new GetTopicByIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el tema.");
            return RedirectToAction(nameof(Index));
        }

        var model = new EditTopicViewModel
        {
            Id = result.Value.StructureTopicId,
            Name = result.Value.Name,
            Description = result.Value.Description,
            StructureModuleId = result.Value.StructureModuleId,
            ModuleName = result.Value.ModuleName,
            KnowledgeStructureName = result.Value.KnowledgeStructureName,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTopicViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateTopicCommand(model.Id, model.Name, model.Description);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema actualizado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<UpdateTopicCommand>(result);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteTopicCommand(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema eliminado exitosamente.");
        }
        else
        {
            this.SetErrorToast("No se pudo eliminar el tema.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetModulesByKnowledgeStructure(long knowledgeStructureId, CancellationToken cancellationToken)
    {
        var query = new GetStructureModulesByKnowledgeStructureQuery(knowledgeStructureId);
        var result = await MediatorExecutor.SendOrThrowAsync(query, cancellationToken);

        var options = result
            .Select(m => new { value = m.StructureModuleId, text = m.ModuleName })
            .ToList();

        return Json(options);
    }

    [HttpGet]
    public async Task<IActionResult> ManageModules(long id, CancellationToken cancellationToken)
    {
        // Get topic with its modules
        var query = new GetTopicModulesQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el tema.");
            return RedirectToAction(nameof(Index));
        }

        // Get all structure modules for selection
        var assignedModuleIds = result.Value.Modules.Select(m => m.StructureModuleId).ToHashSet();
        var availableModules = new List<SelectListItem>();

        // Get all knowledge structures with their modules
        var knowledgeStructures = await MediatorExecutor.SendOrThrowAsync(
            new GetAllKnowledgeStructuresWithHierarchyQuery(),
            cancellationToken);

        foreach (var ks in knowledgeStructures.Where(k => k.IsActive))
        {
            foreach (var module in ks.Modules)
            {
                if (!assignedModuleIds.Contains(module.StructureModuleId))
                {
                    availableModules.Add(new SelectListItem
                    {
                        Value = module.StructureModuleId.ToString(),
                        Text = $"{module.Name} ({ks.Name})",
                    });
                }
            }
        }

        var model = new ManageTopicModulesViewModel
        {
            TopicId = result.Value.TopicId,
            TopicName = result.Value.TopicName,
            AssignedModules = result.Value.Modules,
            AvailableModules = availableModules,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToModule(long topicId, long structureModuleId, CancellationToken cancellationToken)
    {
        var command = new AddTopicToModuleCommand(structureModuleId, topicId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema asignado exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al asignar el tema.");
        }

        return RedirectToAction(nameof(ManageModules), new { id = topicId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromModule(long topicId, long structureTopicId, long structureModuleId, CancellationToken cancellationToken)
    {
        var command = new RemoveTopicFromModuleCommand(structureModuleId, structureTopicId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema removido exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al remover el tema.");
        }

        return RedirectToAction(nameof(ManageModules), new { id = topicId });
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

    private async Task LoadCreateViewModelOptionsAsync(CreateTopicViewModel model, CancellationToken cancellationToken)
    {
        model.KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken);

        if (model.KnowledgeStructureId.HasValue)
        {
            var modulesQuery = new GetStructureModulesByKnowledgeStructureQuery(model.KnowledgeStructureId.Value);
            var modulesResult = await MediatorExecutor.SendOrThrowAsync(modulesQuery, cancellationToken);

            model.ModuleOptions = modulesResult
                .Select(m => new SelectListItem
                {
                    Value = m.StructureModuleId.ToString(),
                    Text = m.ModuleName,
                })
                .ToList();
        }
    }
}
