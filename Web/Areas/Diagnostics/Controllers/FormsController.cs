using LinaSys.Diagnostics.Application.Block.Queries;
using LinaSys.Diagnostics.Application.Form.Commands;
using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.Diagnostics.Application.Question.Commands;
using LinaSys.Diagnostics.Application.Question.Queries;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.Orchestration.Application.Diagnostics.Commands;
using LinaSys.Web.Areas.Diagnostics.Models.Forms;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Diagnostics.Controllers;

[Area("Diagnostics")]
[Route("Diagnostics")]
public class FormsController(ILogger<FormsController> logger, MediatorExecutor mediator, MediatorExecutor directMediator) : AuthorizedBaseController(logger, directMediator)
{
    [HttpGet("Forms/List")]
    public IActionResult List()
    {
        return View();
    }

    [HttpPost("Forms/List")]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListFormsQuery(
            Start: request.Start,
            Length: request.Length,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await mediator.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet("Forms/LoadCSV")]
    public IActionResult LoadCSV()
    {
        return View(new LoadCSVViewModel());
    }

    [HttpPost("Forms/LoadCSV")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadCSV(LoadCSVViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            await using var stream = viewModel.File.OpenReadStream();
            var command = new UpsertDiagnosisFormFromCsvOrchestrationCommand(stream, viewModel.FormName, 1);
            var result = await mediator.SendAndLogIfFailureAsync(command, cancellationToken);

            if (!result.IsFailure)
            {
                this.SetSuccessToast("Carga de formulario completa");
                return RedirectToAction("LoadCSV");
            }

            MapErrorsToModelStateAndSetErrorToast<UpsertDiagnosisFormFromCsvOrchestrationCommand>(result);
        }

        return View(viewModel);
    }

    [HttpGet("Forms/Create")]
    public IActionResult Create()
    {
        return View(new CreateFormViewModel());
    }

    [HttpPost("Forms/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateFormCommand(model.Name);
        var result = await MediatorExecutor.SendOrThrowAsync(command);

        this.SetSuccessToast("Formulario creado exitosamente.");
        return RedirectToAction("Builder", new { id = result });
    }

    [HttpGet("Forms/Builder/{id:long}")]
    public async Task<IActionResult> Builder(long id)
    {
        // Get form details
        var formQuery = new GetFormWithQuestionsAndAnswersQuery(id);
        var formResult = await MediatorExecutor.SendOrThrowAsync(formQuery);

        // Get available blocks
        var blocksQuery = new GetAllBlocksQuery();
        var blocksResult = await MediatorExecutor.SendOrThrowAsync(blocksQuery);

        // Get knowledge structures for module/topic selection
        var knowledgeStructuresQuery = new GetAllKnowledgeStructuresWithHierarchyQuery();
        var knowledgeStructuresResult = await MediatorExecutor.SendOrThrowAsync(knowledgeStructuresQuery);

        var viewModel = new FormBuilderViewModel
        {
            FormId = id,
            FormName = formResult?.Name ?? string.Empty,
            Questions = formResult?.Questions ?? [],
            AvailableBlocks = blocksResult,
            KnowledgeStructures = knowledgeStructuresResult,
        };

        return View(viewModel);
    }

    [HttpPost("Forms/AddQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion([FromBody] AddQuestionToFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Datos inválidos." });
        }

        var command = new AddQuestionToFormCommand(
            model.FormId,
            model.TopicId,
            model.BlockId,
            model.QuestionText,
            model.AnswerType,
            model.QuestionPhase,
            model.IsUsedForMentoringPlan,
            model.IsUsedForDiagnosis,
            model.Order);

        await MediatorExecutor.SendOrThrowAsync(command);

        return Json(new { success = true });
    }

    [HttpPost("Forms/RemoveQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuestion([FromBody] RemoveQuestionFromFormViewModel model)
    {
        var command = new RemoveQuestionFromFormCommand(model.FormId, model.QuestionId);
        await MediatorExecutor.SendOrThrowAsync(command);

        return Json(new { success = true });
    }

    [HttpPost("Forms/UpdateQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Datos inválidos." });
        }

        var answerOptions = MapAnswerOptions(model.AnswerOptions);

        var command = new UpdateQuestionCommand(
            model.QuestionId,
            model.QuestionText,
            (AnswerType)model.AnswerType,
            (LinaSys.Diagnostics.Domain.Enums.QuestionPhase)model.QuestionPhase,
            model.IsUsedForMentoringPlan,
            model.IsUsedForDiagnosis,
            answerOptions);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);

        if (result.IsSuccess)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Error al actualizar la pregunta." });
    }

    [HttpGet("Forms/GetQuestion/{questionId:long}")]
    public async Task<IActionResult> GetQuestion(long questionId)
    {
        var query = new GetQuestionByIdQuery(questionId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

        if (result.IsSuccess && result.Value != null)
        {
            return Json(new
            {
                success = true,
                question = result.Value
            });
        }

        return Json(new { success = false, message = "Pregunta no encontrada." });
    }

    [HttpPost("Forms/ReorderQuestions")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ReorderQuestions([FromBody] ReorderQuestionsViewModel model)
    {
        // This would require a new command to be implemented
        // For now, return success
        return Task.FromResult<IActionResult>(Json(new { success = true }));
    }

    private static List<UpdateAnswerOptionDto>? MapAnswerOptions(List<UpdateAnswerOptionViewModel>? viewModels)
    {
        if (viewModels == null || viewModels.Count == 0)
        {
            return null;
        }

        return viewModels.Select(vm => new UpdateAnswerOptionDto(
            vm.Id,
            vm.Text,
            vm.Score,
            (FodaType)vm.Foda,
            vm.FodaExplanation,
            (OdsrType)vm.Odsr,
            vm.OdsrExplanation,
            vm.FollowupQuestionText,
            vm.Order,
            vm.IsDeleted)).ToList();
    }
}
