using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.KnowledgeStructure.Application.Module.Commands;
using LinaSys.KnowledgeStructure.Application.Module.Queries;
using LinaSys.KnowledgeStructure.Application.Subject.Commands;
using LinaSys.KnowledgeStructure.Application.Subject.Queries;
using LinaSys.KnowledgeStructure.Application.Topic.Commands;
using LinaSys.KnowledgeStructure.Application.Topic.Queries;
using LinaSys.Shared.Application;
using LinaSys.Web.Areas.KnowledgeStructure.Models.KnowledgeStructure;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.KnowledgeStructure.Controllers;

[Area("KnowledgeStructure")]
public class KnowledgeStructureController(ILogger<KnowledgeStructureController> logger, MediatorExecutor mediator)
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
        var query = new GetAllKnowledgeStructuresQuery();
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess)
        {
            return Json(new DataTableResponse<object>
            {
                Draw = request.Draw,
                Data = [],
                RecordsTotal = 0,
                RecordsFiltered = 0,
            });
        }

        var data = result.Value?.Select(ks => new
        {
            ks.Id,
            ks.Name,
            ks.Description,
            ks.IsActive,
            ks.CreatedAt,
        }).Cast<object>().ToList() ?? [];

        return Json(new DataTableResponse<object>
        {
            Draw = request.Draw,
            Data = data,
            RecordsTotal = data.Count,
            RecordsFiltered = data.Count,
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateKnowledgeStructureViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateKnowledgeStructureViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateKnowledgeStructureCommand(
            model.Name,
            model.Description,
            model.IsActive);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Estructura de conocimiento creada exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<CreateKnowledgeStructureCommand>(result);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var query = new GetKnowledgeStructureByIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (!result.IsSuccess || result.Value is null)
        {
            this.SetErrorToast("No se pudo cargar la estructura de conocimiento.");
            return RedirectToAction(nameof(Index));
        }

        var model = new EditKnowledgeStructureViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Description = result.Value.Description,
            IsActive = result.Value.IsActive,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditKnowledgeStructureViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateKnowledgeStructureCommand(
            model.Id,
            model.Name,
            model.Description,
            model.IsActive);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Estructura de conocimiento actualizada exitosamente.");
            return RedirectToAction(nameof(Index));
        }

        MapErrorsToModelStateAndSetErrorToast<UpdateKnowledgeStructureCommand>(result);
        return View(model);
    }

    [HttpGet]
    public IActionResult Builder()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTreeData(string? id)
    {
        if (string.IsNullOrEmpty(id) || id == "#")
        {
            // Get all knowledge structures
            var structures = await MediatorExecutor.SendAndLogIfFailureAsync(new GetAllKnowledgeStructuresQuery());
            if (!structures.IsSuccess)
            {
                return Json(new List<TreeNodeViewModel>());
            }

            var nodes = structures.Value?.Select(ks => new TreeNodeViewModel
            {
                id = $"ks_{ks.Id}",
                text = ks.Name,
                icon = "fa fa-sitemap",
                type = "knowledgestructure",
                data = new { id = ks.Id, type = "knowledgestructure" },
                children = true,
            }).ToList() ?? [];

            return Json(nodes);
        }

        // Parse the id to determine the type
        var parts = id.Split('_');
        if (parts.Length != 2)
        {
            return Json(new List<TreeNodeViewModel>());
        }

        var type = parts[0];
        var entityId = long.Parse(parts[1]);

        return type switch
        {
            "ks" => await GetKnowledgeStructureChildren(entityId),
            "mod" => await GetModuleChildren(entityId),
            "topic" => await GetTopicChildren(entityId),
            _ => Json(new List<TreeNodeViewModel>()),
        };
    }

    [HttpPost]
    public async Task<IActionResult> MoveNode(MoveNodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Datos inválidos" });
        }

        try
        {
            // Parse node and parent IDs
            var nodeParts = model.NodeId.Split('_');
            var nodeType = nodeParts[0];
            var nodeId = long.Parse(nodeParts[1]);

            var parentParts = model.NewParentId.Split('_');
            var parentType = parentParts[0];
            var parentId = long.Parse(parentParts[1]);

            // Validate the move operation
            if (!IsValidMove(nodeType, parentType))
            {
                return Json(new { success = false, message = "Movimiento no válido" });
            }

            // Execute the appropriate move command
            Result result = nodeType switch
            {
                "mod" => await MoveModule(nodeId, parentId, model.Position),
                "topic" => await MoveTopic(nodeId, parentId, model.Position),
                "subject" => await MoveSubject(nodeId, parentId, model.Position),
                _ => Result.Failure(ResultErrorCodes.Unknown, (string.Empty, "Tipo de nodo no válido")),
            };

            if (result.IsSuccess)
            {
                this.SetSuccessToast("Elemento movido correctamente");
                return Json(new { success = true });
            }

            return Json(new { success = false, message = result.ErrorMessages?.FirstOrDefault().Message ?? "Error desconocido" });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Error al mover el elemento" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetNodeDetails(string nodeId)
    {
        var parts = nodeId.Split('_');
        if (parts.Length != 2)
        {
            return PartialView("_NodeDetailsError");
        }

        var type = parts[0];
        var id = long.Parse(parts[1]);

        return type switch
        {
            "ks" => await GetKnowledgeStructureDetails(id),
            "mod" => await GetModuleDetails(id),
            "topic" => await GetTopicDetails(id),
            "subject" => await GetSubjectDetails(id),
            _ => PartialView("_NodeDetailsError"),
        };
    }

    private async Task<IActionResult> GetKnowledgeStructureChildren(long knowledgeStructureId)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new GetModulesTopicsAndSubjectsFromKnowledgeStructure(knowledgeStructureId));
        if (!result.IsSuccess)
        {
            return Json(new List<TreeNodeViewModel>());
        }

        var nodes = result.Value?.Modules?.Select(m => new TreeNodeViewModel
        {
            id = $"mod_{m.StructureModuleId}",
            text = m.Name,
            icon = "fa fa-book",
            type = "module",
            data = new { id = m.StructureModuleId, type = "module", order = m.Order },
            children = m.Topics.Any(),
        }).ToList() ?? [];

        return Json(nodes);
    }

    private async Task<IActionResult> GetModuleChildren(long moduleId)
    {
        // Get the module's topics
        var structureModule = await MediatorExecutor.SendAndLogIfFailureAsync(new GetStructureModuleWithTopicsQuery(moduleId));
        if (!structureModule.IsSuccess)
        {
            return Json(new List<TreeNodeViewModel>());
        }

        var nodes = structureModule.Value?.Topics?.Select(t => new TreeNodeViewModel
        {
            id = $"topic_{t.Id}",
            text = t.Name,
            icon = "fa fa-file-text",
            type = "topic",
            data = new { id = t.Id, type = "topic", order = t.Order },
            children = true, // Topics can have subjects
        }).ToList() ?? [];

        return Json(nodes);
    }

    private async Task<IActionResult> GetTopicChildren(long topicId)
    {
        // Get the topic's subjects
        var topic = await MediatorExecutor.SendAndLogIfFailureAsync(new GetTopicWithSubjectsQuery(topicId));
        if (!topic.IsSuccess)
        {
            return Json(new List<TreeNodeViewModel>());
        }

        var nodes = topic.Value?.Subjects?.Select(s => new TreeNodeViewModel
        {
            id = $"subject_{s.SubjectId}",
            text = s.Title,
            icon = "fa fa-graduation-cap",
            type = "subject",
            data = new { id = s.SubjectId, type = "subject", order = s.Order },
            children = false,
        }).ToList() ?? [];

        return Json(nodes);
    }

    private bool IsValidMove(string nodeType, string parentType)
    {
        return (nodeType, parentType) switch
        {
            ("mod", "ks") => true,
            ("topic", "mod") => true,
            ("subject", "topic") => true,
            _ => false,
        };
    }

    private async Task<Result> MoveModule(long moduleId, long knowledgeStructureId, int position)
    {
        return await MediatorExecutor.SendAndLogIfFailureAsync(new MoveModuleCommand(moduleId, knowledgeStructureId, position));
    }

    private async Task<Result> MoveTopic(long topicId, long moduleId, int position)
    {
        return await MediatorExecutor.SendAndLogIfFailureAsync(new MoveTopicCommand(topicId, moduleId, position));
    }

    private async Task<Result> MoveSubject(long subjectId, long topicId, int position)
    {
        return await MediatorExecutor.SendAndLogIfFailureAsync(new MoveSubjectCommand(subjectId, topicId, position));
    }

    private async Task<IActionResult> GetKnowledgeStructureDetails(long id)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new GetKnowledgeStructureByIdQuery(id));
        if (!result.IsSuccess)
        {
            return PartialView("_NodeDetailsError");
        }

        return PartialView("_KnowledgeStructureDetails", result.Value);
    }

    private async Task<IActionResult> GetModuleDetails(long id)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new GetModuleDetailsQuery(id));
        if (!result.IsSuccess)
        {
            return PartialView("_NodeDetailsError");
        }

        return PartialView("_ModuleDetails", result.Value);
    }

    private async Task<IActionResult> GetTopicDetails(long id)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new GetTopicDetailsQuery(id));
        if (!result.IsSuccess)
        {
            return PartialView("_NodeDetailsError");
        }

        return PartialView("_TopicDetails", result.Value);
    }

    private async Task<IActionResult> GetSubjectDetails(long id)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new GetSubjectDetailsQuery(id));
        if (!result.IsSuccess)
        {
            return PartialView("_NodeDetailsError");
        }

        return PartialView("_SubjectDetails", result.Value);
    }
}
