using LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectAnswerOption;
using LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectBlock;
using LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectQuestion;
using LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectAnswerOption;
using LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectBlock;
using LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectQuestion;
using LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectAnswerOption;
using LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectBlock;
using LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectQuestion;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.ClearProjectKnowledgeStructure;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectModule;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectSubject;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectTopic;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectModule;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectSubject;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectTopic;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.MoveProjectNode;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncAllProjectKnowledgeStructure;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectModule;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectSubject;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectTopic;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectModule;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectSubject;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectTopic;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectBlocksTree;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructure;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectsWithKnowledgeStructure;
using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.Orchestration.Application.BusinessIncubator.Commands;
using LinaSys.Shared.Infrastructure.Extensions;
using LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;
using LinaSys.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

/// <summary>
/// Controller for managing project-specific knowledge structures.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProjectKnowledgeStructureController"/> class.
/// </remarks>
/// <param name="mediator">The mediator.</param>
[Area("BusinessIncubators")]
[Route("BusinessIncubators/{businessIncubatorId:guid}/Projects/{projectId:guid}/KnowledgeStructure")]
public class ProjectKnowledgeStructureController(IMediator mediator) : Controller
{

    /// <summary>
    /// Displays the project knowledge structure management page.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The view result.</returns>
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid businessIncubatorId, Guid projectId)
    {
        // Get knowledge structure details
        var query = new GetProjectKnowledgeStructureQuery(businessIncubatorId, projectId);
        var result = await mediator.Send(query);

        var model = new ProjectKnowledgeStructureViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            ProjectId = projectId,
            ProjectName = result.IsSuccess && result.Value is not null ? result.Value.ProjectName : "Proyecto",
            HasKnowledgeStructure = result.IsSuccess && result.Value is not null,
            SourceFormId = result.IsSuccess && result.Value is not null ? result.Value.SourceKnowledgeStructureId : null,
            SourceFormName = result.IsSuccess && result.Value is not null && result.Value.SourceKnowledgeStructureId.HasValue ? result.Value.Name : null
        };

        return View(model);
    }

    /// <summary>
    /// Shows the form selection modal for copying a knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The partial view result.</returns>
    [HttpGet("SelectSourceForm")]
    public IActionResult SelectSourceForm(Guid businessIncubatorId, Guid projectId)
    {
        var model = new SelectSourceFormViewModel
        {
            BusinessIncubatorId = businessIncubatorId,
            ProjectId = projectId
        };

        // This will be populated dynamically based on the source type selection
        // For "global": Get forms from Diagnostics module
        // For "project": Get other projects from the same incubator that have knowledge structures
        model.AvailableForms = [];

        return PartialView("_SelectSourceForm", model);
    }

    /// <summary>
    /// Gets available sources based on the source type.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="sourceType">The source type (global or project).</param>
    /// <returns>The available sources as JSON.</returns>
    [HttpGet("GetAvailableSources")]
    public async Task<IActionResult> GetAvailableSources(Guid businessIncubatorId, Guid projectId, int sourceType)
    {
        var sources = new List<object>();
        var sourceTypeEnum = (BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType)sourceType;

        if (sourceTypeEnum == BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType.Global)
        {
            // Get forms from Diagnostics module
            var formsQuery = new ListFormsQuery(0, 100, null, "Name", "asc");
            var formsResult = await mediator.Send(formsQuery);

            if (formsResult.IsSuccess && formsResult.Value?.Data.Any() == true)
            {
                sources = formsResult.Value.Data
                    .Select(f => new
                    {
                        id = f.Id,
                        name = f.Name,
                        type = "global"
                    })
                    .Cast<object>()
                    .ToList();
            }
        }
        else if (sourceTypeEnum == BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType.Project)
        {
            // Get other projects from the same incubator that have knowledge structures
            var projectsQuery = new GetProjectsWithKnowledgeStructureQuery(businessIncubatorId, projectId);
            var projectsResult = await mediator.Send(projectsQuery);

            if (projectsResult.IsSuccess && projectsResult.Value?.Any() == true)
            {
                sources = projectsResult.Value
                    .Select(p => new
                    {
                        id = p.Id,
                        name = $"{p.Name} - {p.KnowledgeStructureName} ({p.ModuleCount} módulos, {p.TopicCount} temas, {p.SubjectCount} materias)",
                        type = "project"
                    })
                    .Cast<object>()
                    .ToList();
            }
        }

        return Json(sources);
    }

    /// <summary>
    /// Gets a preview of the knowledge structure from a source.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="sourceId">The source ID.</param>
    /// <returns>The preview data as JSON.</returns>
    [HttpGet("GetSourcePreview")]
    public async Task<IActionResult> GetSourcePreview(
        Guid businessIncubatorId,
        Guid projectId,
        int sourceType,
        long sourceId)
    {
        var sourceTypeEnum = (BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType)sourceType;
        var modules = new List<object>();
        var totalQuestions = 0;
        var message = string.Empty;

        if (sourceTypeEnum == BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType.Global)
        {
            // Get form structure from Diagnostics
            var formQuery = new GetFormWithQuestionsAndAnswersQuery(sourceId);
            var formResult = await mediator.Send(formQuery);

            if (formResult.IsSuccess && formResult.Value is not null)
            {
                var form = formResult.Value;
                var blockGroups = form.Questions
                    .GroupBy(q => q.BlockId)
                    .OrderBy(g => g.First().BlockName);

                foreach (var blockGroup in blockGroups)
                {
                    var blockName = blockGroup.First().BlockName;
                    var questions = blockGroup.ToList();

                    modules.Add(new
                    {
                        name = blockName,
                        questionsCount = questions.Count,
                        // Note: Questions in diagnostics may not have topics
                        hasTopics = false
                    });
                }

                totalQuestions = form.Questions.Count;
                message = $"Se crearán {blockGroups.Count()} módulos basados en los bloques del formulario";
            }
        }
        else if (sourceTypeEnum == BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType.Project)
        {
            // Get structure from another project
            // sourceId is the project's internal ID, we need to get the external ID
            var projectsQuery = new GetProjectsWithKnowledgeStructureQuery(businessIncubatorId, projectId);
            var projectsResult = await mediator.Send(projectsQuery);

            if (projectsResult.IsSuccess && projectsResult.Value is not null)
            {
                var sourceProject = projectsResult.Value.FirstOrDefault(p => p.Id == sourceId);
                if (sourceProject is not null)
                {
                    // For now, just show basic info since we need the project's external ID
                    modules.Add(new
                    {
                        name = sourceProject.KnowledgeStructureName,
                        topicsCount = sourceProject.TopicCount,
                        questionsCount = 0 // Would need separate query
                    });

                    message = $"Se copiará la estructura '{sourceProject.KnowledgeStructureName}' con {sourceProject.ModuleCount} módulos, {sourceProject.TopicCount} temas y {sourceProject.SubjectCount} materias";
                }
            }
        }

        var preview = new
        {
            modules = modules,
            totalQuestions = totalQuestions,
            message = message
        };

        return Json(preview);
    }

    /// <summary>
    /// Copies a knowledge structure from a source form to the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The copy form model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("CopyStructure")]
    public async Task<IActionResult> CopyStructure(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CopyKnowledgeStructureModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Use the orchestration command that handles all source types
        var command = new CopyKnowledgeStructureToProjectCommand(
            businessIncubatorId,
            projectId,
            (BusinessIncubator.Domain.Enums.KnowledgeStructureSourceType)model.SourceType,
            model.SourceFormId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al copiar la estructura";
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Estructura copiada exitosamente" });
    }

    /// <summary>
    /// Gets the knowledge structure tree data for the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The tree data as JSON.</returns>
    [HttpGet("Tree")]
    public async Task<IActionResult> Tree(
        Guid businessIncubatorId,
        Guid projectId)
    {
        var query = new GetProjectKnowledgeStructureTreeQuery(businessIncubatorId, projectId);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return Json(new[]
            {
                new
                {
                    id = "root",
                    text = "Error al cargar estructura",
                    type = "root",
                    children = false
                }
            });
        }

        var treeNodes = result.Value;
        return Json(treeNodes);
    }

    /// <summary>
    /// Gets the project blocks tree data for the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The tree data as JSON.</returns>
    [HttpGet("BlocksTree")]
    public async Task<IActionResult> BlocksTree(
        Guid businessIncubatorId,
        Guid projectId)
    {
        var query = new GetProjectBlocksTreeQuery(businessIncubatorId, projectId);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return Json(new[]
            {
                new
                {
                    id = "root",
                    text = "Error al cargar bloques",
                    type = "root",
                    children = false
                }
            });
        }

        var treeNodes = result.Value;
        return Json(treeNodes);
    }

    /// <summary>
    /// Updates a module in the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="moduleId">The module ID.</param>
    /// <param name="model">The update model.</param>
    /// <returns>The action result.</returns>
    [HttpPut("modules/{moduleId:long}")]
    public async Task<IActionResult> UpdateModule(
        Guid businessIncubatorId,
        Guid projectId,
        long moduleId,
        [FromBody] UpdateModuleModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateProjectModuleCommand(
            businessIncubatorId,
            projectId,
            moduleId,
            model.Name,
            model.Order);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al actualizar el módulo" });
        }

        return Json(new { success = true, message = "Módulo actualizado exitosamente" });
    }

    /// <summary>
    /// Syncs a module with its source.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="moduleId">The module ID.</param>
    /// <returns>The action result.</returns>
    [HttpPost("modules/{moduleId:long}/sync")]
    public async Task<IActionResult> SyncModule(
        Guid businessIncubatorId,
        Guid projectId,
        long moduleId)
    {
        var command = new SyncProjectModuleCommand(businessIncubatorId, projectId, moduleId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al sincronizar el módulo" });
        }

        return Json(new { success = true, message = "Módulo sincronizado exitosamente" });
    }

    /// <summary>
    /// Syncs all non-customized elements in the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The action result.</returns>
    [HttpPost("SyncAll")]
    public async Task<IActionResult> SyncAll(
        Guid businessIncubatorId,
        Guid projectId)
    {
        var command = new SyncAllProjectKnowledgeStructureCommand(businessIncubatorId, projectId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al sincronizar la estructura" });
        }

        return Json(new { success = true, message = "Estructura sincronizada exitosamente" });
    }

    /// <summary>
    /// Syncs a topic with its source.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <returns>The action result.</returns>
    [HttpPost("topics/{topicId:long}/sync")]
    public async Task<IActionResult> SyncTopic(
        Guid businessIncubatorId,
        Guid projectId,
        long topicId)
    {
        var command = new SyncProjectTopicCommand(businessIncubatorId, projectId, topicId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al sincronizar el tema" });
        }

        return Json(new { success = true, message = "Tema sincronizado exitosamente" });
    }

    /// <summary>
    /// Syncs a subject with its source.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="subjectId">The subject ID.</param>
    /// <returns>The action result.</returns>
    [HttpPost("subjects/{subjectId:long}/sync")]
    public async Task<IActionResult> SyncSubject(
        Guid businessIncubatorId,
        Guid projectId,
        long subjectId)
    {
        var command = new SyncProjectSubjectCommand(businessIncubatorId, projectId, subjectId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al sincronizar la materia" });
        }

        return Json(new { success = true, message = "Materia sincronizada exitosamente" });
    }

    /// <summary>
    /// Clears the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("Clear")]
    public async Task<IActionResult> Clear(
        Guid businessIncubatorId,
        Guid projectId)
    {
        var command = new ClearProjectKnowledgeStructureCommand(businessIncubatorId, projectId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = "Error al limpiar la estructura" });
        }

        return Json(new { success = true, message = "Estructura limpiada exitosamente" });
    }

    /// <summary>
    /// Moves a node in the knowledge structure tree.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The move node model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("MoveNode")]
    public async Task<IActionResult> MoveNode(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] MoveNodeModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { field = x.Key, errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            var errorMessage = errors.Any()
                ? string.Join(", ", errors.SelectMany(e => e.errors))
                : "Datos inválidos";

            return Json(new { success = false, message = errorMessage, validationErrors = errors });
        }

        var command = new MoveProjectNodeCommand(
            businessIncubatorId,
            projectId,
            $"{model.NodeType}_{model.NodeId}",
            model.NodeType,
            model.NewParentId?.ToString(),
            null, // ParentType not provided in model
            model.NewPosition);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al mover el elemento" });
        }

        return Json(new { success = true, message = "Elemento movido exitosamente" });
    }

    /// <summary>
    /// Creates a new block in the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The create block model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("blocks")]
    public async Task<IActionResult> CreateBlock(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CreateBlockModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateProjectBlockCommand(
            businessIncubatorId,
            projectId,
            model.Name,
            model.Order ?? 999);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al crear el bloque" });
        }

        return Json(new { success = true, message = "Bloque creado exitosamente", id = result.Value });
    }

    /// <summary>
    /// Gets a question by ID.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="questionId">The question ID.</param>
    /// <returns>The question data as JSON.</returns>
    [HttpGet("questions/{questionId:long}")]
    public async Task<IActionResult> GetQuestion(
        Guid businessIncubatorId,
        Guid projectId,
        long questionId)
    {
        // Get the project with blocks
        var projectQuery = new GetProjectBlocksTreeQuery(businessIncubatorId, projectId);
        var projectResult = await mediator.Send(projectQuery);

        if (!projectResult.IsSuccess || projectResult.Value is null)
        {
            return NotFound(new { success = false, message = "Proyecto no encontrado" });
        }

        // Find the question in the tree
        foreach (var rootNode in projectResult.Value)
        {
            if (rootNode.Type == "root" && rootNode.Children is not null)
            {
                foreach (var blockNode in rootNode.Children)
                {
                    if (blockNode.Type == "block" && blockNode.Children is not null)
                    {
                        foreach (var questionNode in blockNode.Children)
                        {
                            if (questionNode.Type == "question" && questionNode.Data?.EntityId == questionId)
                            {
                                // Parse enum values from strings to integers
                                var answerTypeValue = 0;
                                if (!string.IsNullOrEmpty(questionNode.Data?.AnswerType) &&
                                    Enum.TryParse<BusinessIncubator.Domain.Enums.AnswerType>(questionNode.Data.AnswerType, out var parsedAnswerType))
                                {
                                    answerTypeValue = (int)parsedAnswerType;
                                }

                                var appliesToPhaseValue = 0;
                                if (!string.IsNullOrEmpty(questionNode.Data?.AppliesToPhase) &&
                                    Enum.TryParse<BusinessIncubator.Domain.Enums.QuestionPhase>(questionNode.Data.AppliesToPhase, out var parsedPhase))
                                {
                                    appliesToPhaseValue = (int)parsedPhase;
                                }

                                return Json(new
                                {
                                    success = true,
                                    question = new
                                    {
                                        id = questionId,
                                        text = questionNode.Text,
                                        blockId = blockNode.Data?.EntityId,
                                        answerType = answerTypeValue,
                                        isUsedForDiagnosis = questionNode.Data?.IsUsedForDiagnosis ?? false,
                                        appliesToPhase = appliesToPhaseValue,
                                        topicId = questionNode.Data?.TopicId,
                                        order = questionNode.Data?.Order ?? 999
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        return NotFound(new { success = false, message = "Pregunta no encontrada" });
    }

    /// <summary>
    /// Creates a new question in a block.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The create question model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CreateQuestionModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateProjectQuestionCommand(
            businessIncubatorId,
            projectId,
            model.BlockId,
            model.Text,
            (BusinessIncubator.Domain.Enums.AnswerType)model.AnswerType,
            model.IsUsedForDiagnosis,
            (BusinessIncubator.Domain.Enums.QuestionPhase)model.AppliesToPhase,
            model.Order ?? 999,
            model.TopicId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al crear la pregunta" });
        }

        return Json(new { success = true, message = "Pregunta creada exitosamente", id = result.Value });
    }

    /// <summary>
    /// Creates a new module in the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The create module model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("modules")]
    public async Task<IActionResult> CreateModule(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CreateModuleModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateProjectModuleCommand(
            businessIncubatorId,
            projectId,
            model.Name,
            model.Order ?? 999);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessages?.FirstOrDefault().Message ?? "Error al crear el módulo" });
        }

        return Json(new { success = true, message = "Módulo creado exitosamente", id = result.Value });
    }

    /// <summary>
    /// Creates a new topic in a module.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The create topic model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("topics")]
    public async Task<IActionResult> CreateTopic(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CreateTopicModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateProjectTopicCommand(
            businessIncubatorId,
            projectId,
            model.ModuleId,
            model.Name,
            model.Order ?? 999);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al crear el tema" });
        }

        return Json(new { success = true, message = "Tema creado exitosamente", id = result.Value });
    }

    /// <summary>
    /// Creates a new subject in a topic.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="model">The create subject model.</param>
    /// <returns>The action result.</returns>
    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject(
        Guid businessIncubatorId,
        Guid projectId,
        [FromBody] CreateSubjectModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateProjectSubjectCommand(
            businessIncubatorId,
            projectId,
            model.TopicId,
            model.Name,
            model.Content,
            model.Order ?? 999);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al crear la materia" });
        }

        return Json(new { success = true, message = "Materia creada exitosamente", id = result.Value });
    }

    /// <summary>
    /// Updates a block in the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="blockId">The block ID.</param>
    /// <param name="model">The update block model.</param>
    /// <returns>The action result.</returns>
    [HttpPut("blocks/{blockId:long}")]
    public async Task<IActionResult> UpdateBlock(
        Guid businessIncubatorId,
        Guid projectId,
        long blockId,
        [FromBody] UpdateBlockModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateProjectBlockCommand(
            businessIncubatorId,
            projectId,
            blockId,
            model.Name,
            model.Order);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al actualizar el bloque" });
        }

        return Json(new { success = true, message = "Bloque actualizado exitosamente" });
    }

    /// <summary>
    /// Updates a question in the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="questionId">The question ID.</param>
    /// <param name="model">The update question model.</param>
    /// <returns>The action result.</returns>
    [HttpPut("questions/{questionId:long}")]
    public async Task<IActionResult> UpdateQuestion(
        Guid businessIncubatorId,
        Guid projectId,
        long questionId,
        [FromBody] UpdateQuestionModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateProjectQuestionCommand(
            businessIncubatorId,
            projectId,
            questionId,
            model.Text,
            (BusinessIncubator.Domain.Enums.AnswerType)model.AnswerType,
            model.IsUsedForDiagnosis,
            (BusinessIncubator.Domain.Enums.QuestionPhase)model.AppliesToPhase,
            model.Order ?? 999,
            model.TopicId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al actualizar la pregunta" });
        }

        return Json(new { success = true, message = "Pregunta actualizada exitosamente" });
    }

    /// <summary>
    /// Updates a topic in the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="model">The update topic model.</param>
    /// <returns>The action result.</returns>
    [HttpPut("topics/{topicId:long}")]
    public async Task<IActionResult> UpdateTopic(
        Guid businessIncubatorId,
        Guid projectId,
        long topicId,
        [FromBody] UpdateTopicModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateProjectTopicCommand(
            businessIncubatorId,
            projectId,
            topicId,
            model.Name,
            model.Order);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al actualizar el tema" });
        }

        return Json(new { success = true, message = "Tema actualizado exitosamente" });
    }

    /// <summary>
    /// Updates a subject in the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="subjectId">The subject ID.</param>
    /// <param name="model">The update subject model.</param>
    /// <returns>The action result.</returns>
    [HttpPut("subjects/{subjectId:long}")]
    public async Task<IActionResult> UpdateSubject(
        Guid businessIncubatorId,
        Guid projectId,
        long subjectId,
        [FromBody] UpdateSubjectModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateProjectSubjectCommand(
            businessIncubatorId,
            projectId,
            subjectId,
            model.Name,
            model.Content,
            model.Order);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al actualizar la materia" });
        }

        return Json(new { success = true, message = "Materia actualizada exitosamente" });
    }

    /// <summary>
    /// Deletes a block from the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="blockId">The block ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("blocks/{blockId:long}")]
    public async Task<IActionResult> DeleteBlock(
        Guid businessIncubatorId,
        Guid projectId,
        long blockId)
    {
        var command = new DeleteProjectBlockCommand(
            businessIncubatorId,
            projectId,
            blockId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar el bloque" });
        }

        return Json(new { success = true, message = "Bloque eliminado exitosamente" });
    }

    /// <summary>
    /// Deletes a question from the project.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="questionId">The question ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("questions/{questionId:long}")]
    public async Task<IActionResult> DeleteQuestion(
        Guid businessIncubatorId,
        Guid projectId,
        long questionId)
    {
        var command = new DeleteProjectQuestionCommand(
            businessIncubatorId,
            projectId,
            questionId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar la pregunta" });
        }

        return Json(new { success = true, message = "Pregunta eliminada exitosamente" });
    }

    /// <summary>
    /// Deletes a module from the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="moduleId">The module ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("modules/{moduleId:long}")]
    public async Task<IActionResult> DeleteModule(
        Guid businessIncubatorId,
        Guid projectId,
        long moduleId)
    {
        var command = new DeleteProjectModuleCommand(
            businessIncubatorId,
            projectId,
            moduleId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar el módulo" });
        }

        return Json(new { success = true, message = "Módulo eliminado exitosamente" });
    }

    /// <summary>
    /// Deletes a topic from the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("topics/{topicId:long}")]
    public async Task<IActionResult> DeleteTopic(
        Guid businessIncubatorId,
        Guid projectId,
        long topicId)
    {
        var command = new DeleteProjectTopicCommand(
            businessIncubatorId,
            projectId,
            topicId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar el tema" });
        }

        return Json(new { success = true, message = "Tema eliminado exitosamente" });
    }

    /// <summary>
    /// Deletes a subject from the project knowledge structure.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="subjectId">The subject ID.</param>
    /// <returns>The action result.</returns>
    [HttpDelete("subjects/{subjectId:long}")]
    public async Task<IActionResult> DeleteSubject(
        Guid businessIncubatorId,
        Guid projectId,
        long subjectId)
    {
        var command = new DeleteProjectSubjectCommand(
            businessIncubatorId,
            projectId,
            subjectId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar la materia" });
        }

        return Json(new { success = true, message = "Materia eliminada exitosamente" });
    }

    /// <summary>
    /// Creates a new answer option for a question.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="questionId">The question ID.</param>
    /// <param name="model">The create answer option model.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPost("questions/{questionId:long}/answer-options")]
    public async Task<IActionResult> CreateAnswerOption(
        Guid businessIncubatorId,
        Guid projectId,
        long questionId,
        [FromBody] CreateAnswerOptionModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { field = x.Key, errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            var errorMessage = errors.Any()
                ? string.Join(", ", errors.SelectMany(e => e.errors))
                : "Datos inválidos";

            return Json(new { success = false, message = errorMessage, validationErrors = errors });
        }

        var command = new CreateProjectAnswerOptionCommand(
            businessIncubatorId,
            projectId,
            questionId,
            model.Text,
            model.Score,
            model.Foda,
            model.FodaExplanation,
            model.Odsr,
            model.OdsrExplanation,
            model.Order,
            model.FollowUpQuestionText);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al crear la opción de respuesta" });
        }

        return Json(new { success = true, message = "Opción de respuesta creada exitosamente", id = result.Value });
    }

    /// <summary>
    /// Updates an answer option.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="answerOptionId">The answer option ID.</param>
    /// <param name="model">The update answer option model.</param>
    /// <returns>The result of the operation.</returns>
    [HttpPut("answer-options/{answerOptionId:long}")]
    public async Task<IActionResult> UpdateAnswerOption(
        Guid businessIncubatorId,
        Guid projectId,
        long answerOptionId,
        [FromBody] UpdateAnswerOptionModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { field = x.Key, errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            var errorMessage = errors.Any()
                ? string.Join(", ", errors.SelectMany(e => e.errors))
                : "Datos inválidos";

            return Json(new { success = false, message = errorMessage, validationErrors = errors });
        }

        var command = new UpdateProjectAnswerOptionCommand(
            businessIncubatorId,
            projectId,
            answerOptionId,
            model.Text,
            model.Score,
            model.Foda,
            model.FodaExplanation,
            model.Odsr,
            model.OdsrExplanation,
            model.Order,
            model.FollowUpQuestionText);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al actualizar la opción de respuesta" });
        }

        return Json(new { success = true, message = "Opción de respuesta actualizada exitosamente" });
    }

    /// <summary>
    /// Deletes an answer option.
    /// </summary>
    /// <param name="businessIncubatorId">The business incubator ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="answerOptionId">The answer option ID.</param>
    /// <returns>The result of the operation.</returns>
    [HttpDelete("answer-options/{answerOptionId:long}")]
    public async Task<IActionResult> DeleteAnswerOption(
        Guid businessIncubatorId,
        Guid projectId,
        long answerOptionId)
    {
        var command = new DeleteProjectAnswerOptionCommand(
            businessIncubatorId,
            projectId,
            answerOptionId);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.ErrorMessages?.FirstOrDefault();
            return Json(new { success = false, message = errorMessage?.Message ?? "Error al eliminar la opción de respuesta" });
        }

        return Json(new { success = true, message = "Opción de respuesta eliminada exitosamente" });
    }
}
