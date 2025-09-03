using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.KnowledgeStructure.Application.Module.Queries;
using LinaSys.KnowledgeStructure.Application.Subject.Commands;
using LinaSys.KnowledgeStructure.Application.Subject.Queries;
using LinaSys.KnowledgeStructure.Application.Topic.Commands;
using LinaSys.KnowledgeStructure.Application.Topic.Queries;
using LinaSys.Orchestration.Application.KnowledgeStructure.Commands;
using LinaSys.Web.Areas.KnowledgeStructure.Models.Subject;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.KnowledgeStructure.Controllers;

[Area("KnowledgeStructure")]
public class SubjectsController(ILogger<SubjectsController> logger, MediatorExecutor mediator)
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
        var query = new ListSubjectsQuery(
            Start: request.Start,
            Length: request.Length,
            GlobalSearch: request.GlobalSearch,
            Title: request.ColumnSearches.GetValueOrDefault("title"),
            StructureTopicId: long.TryParse(request.ColumnSearches.GetValueOrDefault("structureTopicId"), out var topicId) ? topicId : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateSubjectViewModel
        {
            KnowledgeStructureOptions = await GetKnowledgeStructureOptionsAsync(cancellationToken),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSubjectViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCreateViewModelOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var command = new CreateSubjectOrchestrationCommand(model.Title, model.Content, model.StructureTopicId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema creado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<CreateSubjectOrchestrationCommand>(result);
        await LoadCreateViewModelOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var query = new GetSubjectByIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el tema.");
            return RedirectToAction(nameof(Index));
        }

        var model = new EditSubjectViewModel
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Content = result.Value.Content,
            StructureTopicId = result.Value.TopicId,
            TopicName = result.Value.TopicName,
            ModuleName = result.Value.ModuleName,
            KnowledgeStructureName = result.Value.KnowledgeStructureName,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSubjectViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateSubjectCommand(model.Id, model.Title, model.Content);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Tema actualizado exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<UpdateSubjectCommand>(result);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteSubjectOrchestrationCommand(id);
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
    public async Task<IActionResult> GetTopicsByModule(long structureModuleId, CancellationToken cancellationToken)
    {
        var query = new GetStructureTopicsByStructureModuleQuery(structureModuleId);
        var result = await MediatorExecutor.SendOrThrowAsync(query, cancellationToken);

        var options = result
            .Select(t => new { value = t.StructureTopicId, text = t.TopicName })
            .ToList();

        return Json(options);
    }

    [HttpGet]
    public async Task<IActionResult> ManageTopics(long id, CancellationToken cancellationToken)
    {
        // Get subject with its topics
        var query = new GetSubjectTopicsQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar la materia.");
            return RedirectToAction(nameof(Index));
        }

        // Get all structure topics for selection
        var allTopicsQuery = new ListTopicsQuery(0, int.MaxValue, null, null, null, null, null, null);
        var allTopicsResult = await MediatorExecutor.SendOrThrowAsync(allTopicsQuery, cancellationToken);

        var assignedTopicIds = result.Value.Topics.Select(t => t.StructureTopicId).ToHashSet();
        var availableTopics = allTopicsResult.Data
            .Where(t => !assignedTopicIds.Contains(t.StructureTopicId))
            .Select(t => new SelectListItem
            {
                Value = t.StructureTopicId.ToString(),
                Text = $"{t.Name} ({t.ModuleName} - {t.KnowledgeStructureName})",
            })
            .OrderBy(t => t.Text)
            .ToList();

        var model = new ManageSubjectTopicsViewModel
        {
            SubjectId = result.Value.SubjectId,
            SubjectName = result.Value.SubjectName,
            SubjectDescription = result.Value.SubjectDescription,
            AssignedTopics = result.Value.Topics,
            AvailableTopics = availableTopics,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToTopic(long subjectId, long structureTopicId, CancellationToken cancellationToken)
    {
        var command = new AddSubjectToTopicCommand(structureTopicId, subjectId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Materia asignada exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al asignar la materia.");
        }

        return RedirectToAction(nameof(ManageTopics), new { id = subjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromTopic(long subjectId, long structureTopicId, CancellationToken cancellationToken)
    {
        var command = new RemoveSubjectFromTopicCommand(structureTopicId, subjectId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Materia removida exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al remover la materia.");
        }

        return RedirectToAction(nameof(ManageTopics), new { id = subjectId });
    }

    [HttpGet]
    public async Task<IActionResult> Resources(long subjectId, CancellationToken cancellationToken)
    {
        var query = new GetSubjectByIdQuery(subjectId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar el tema.");
            return RedirectToAction(nameof(Index));
        }

        var model = new ManageSubjectResourcesViewModel
        {
            SubjectId = result.Value.Id,
            SubjectTitle = result.Value.Title,
            Resources = result.Value.Resources.OrderBy(r => r.Order).ToList(),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResource(AddSubjectResourceViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Resources), new { subjectId = model.SubjectId });
        }

        var command = new AddSubjectResourceCommand(
            model.SubjectId,
            model.Title,
            model.Url,
            model.Type,
            model.EstimatedMinutes);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Recurso agregado exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al agregar el recurso.");
        }

        return RedirectToAction(nameof(Resources), new { subjectId = model.SubjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveResource(long subjectId, long resourceId, CancellationToken cancellationToken)
    {
        var command = new RemoveSubjectResourceCommand(subjectId, resourceId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Recurso eliminado exitosamente.");
        }
        else
        {
            this.SetErrorToast("Error al eliminar el recurso.");
        }

        return RedirectToAction(nameof(Resources), new { subjectId });
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

    private async Task LoadCreateViewModelOptionsAsync(CreateSubjectViewModel model, CancellationToken cancellationToken)
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

            if (model.StructureModuleId.HasValue)
            {
                var topicsQuery = new GetStructureTopicsByStructureModuleQuery(model.StructureModuleId.Value);
                var topicsResult = await MediatorExecutor.SendOrThrowAsync(topicsQuery, cancellationToken);

                model.TopicOptions = topicsResult
                    .Select(t => new SelectListItem
                    {
                        Value = t.StructureTopicId.ToString(),
                        Text = t.TopicName,
                    })
                    .ToList();
            }
        }
    }
}
