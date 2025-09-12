using LinaSys.Diagnostics.Application.Block.Queries;
using LinaSys.Diagnostics.Application.Form.Queries;
using LinaSys.Diagnostics.Application.Question.Commands;
using LinaSys.Diagnostics.Application.Question.Queries;
using LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Queries;
using LinaSys.Web.Areas.Diagnostics.Models.Questions;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Diagnostics.Controllers;

[Area("Diagnostics")]
[Route("Diagnostics")]
public class QuestionsController(ILogger<QuestionsController> logger, MediatorExecutor mediator, IApplicationUrlService applicationUrlService)
    : AuthorizedBaseController(logger, mediator, applicationUrlService)
{
    [HttpGet("Questions/List")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var (topics, knowledgeStructures, subjects) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);

        var model = new QuestionsListViewModel
        {
            AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false),
            Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false),
            QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false),
            Topics = topics,
            KnowledgeStructures = knowledgeStructures,
            Subjects = subjects,
        };

        return View(model);
    }

    [HttpPost("Questions/List")]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListQuestionsQuery(
            Start: request.Start,
            Length: request.Length,
            FormId: int.TryParse(request.ColumnSearches.GetValueOrDefault("formId"), out var f) ? f : null,
            Text: request.ColumnSearches.GetValueOrDefault("text"),
            AnswerType: int.TryParse(request.ColumnSearches.GetValueOrDefault("answerType"), out var a) ? a : null,
            AppliesToPhase: int.TryParse(request.ColumnSearches.GetValueOrDefault("appliesToPhase"), out var at) ? at : null,
            IsUsedForMentoringPlan: bool.TryParse(request.ColumnSearches.GetValueOrDefault("isUsedForMentoringPlan"), out var i) ? i : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpGet("Questions/Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var (topics, knowledgeStructures, subjects) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);

        var model = new CreateQuestionViewModel
        {
            AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false),
            Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false),
            QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false),
            Topics = topics,
            KnowledgeStructures = knowledgeStructures,
            Subjects = subjects,
        };

        return View(model);
    }

    [HttpPost("Questions/Create")]
    public async Task<IActionResult> Create(CreateQuestionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            var (topics, knowledgeStructures, subjects) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);
            model.AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false);
            model.Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false);
            model.QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false);
            model.Topics = topics;
            model.KnowledgeStructures = knowledgeStructures;
            model.Subjects = subjects;
            return View(model);
        }

        var command = new CreateQuestionCommand(
            Text: model.Text,
            AnswerType: model.AnswerType,
            AppliesToPhase: model.AppliesToPhase,
            IsUsedForMentoringPlan: model.IsUsedForMentoringPlan,
            IsUsedForDiagnosis: model.IsUsedForDiagnosis,
            TopicId: model.TopicId,
            BlockId: model.BlockId,
            AnswerOptions: model.AnswerOptions?.Select(o => new AnswerOptionDto(
                Text: o.Text,
                Score: o.Score,
                Foda: o.Foda,
                FodaExplanation: o.FodaExplanation,
                Odsr: o.Odsr,
                OdsrExplanation: o.OdsrExplanation,
                FollowupQuestionText: o.FollowupQuestionText,
                Order: o.Order)).ToList());

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Pregunta creada exitosamente.");
            return RedirectToAction(nameof(List));
        }

        this.MapErrorsToModelStateAndSetErrorToast<CreateQuestionViewModel>(result);

        // Reload dropdowns
        var (topics2, knowledgeStructures2, subjects2) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);
        model.AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false);
        model.Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false);
        model.QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false);
        model.Topics = topics2;
        model.KnowledgeStructures = knowledgeStructures2;
        model.Subjects = subjects2;

        return View(model);
    }

    [HttpGet("Questions/Edit/{id}")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var query = new GetQuestionByIdQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("La pregunta no existe.");
            return RedirectToAction(nameof(List));
        }

        var question = result.Value!;
        var (topics, knowledgeStructures, subjects) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);

        var model = new EditQuestionViewModel
        {
            Id = question.Id,
            Text = question.Text,
            AnswerType = question.AnswerType,
            AppliesToPhase = question.AppliesToPhase,
            IsUsedForMentoringPlan = question.IsUsedForMentoringPlan,
            IsUsedForDiagnosis = question.IsUsedForDiagnosis,
            AnswerOptions = question.AnswerOptions?.Select(o => new AnswerOptionViewModel
            {
                Id = o.Id,
                Text = o.Text,
                Score = o.Score,
                Foda = o.Foda,
                FodaExplanation = o.FodaExplanation,
                Odsr = o.Odsr,
                OdsrExplanation = o.OdsrExplanation,
                FollowupQuestionText = o.FollowupQuestionText,
                Order = o.Order
            }).ToList(),
            AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false),
            Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false),
            QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false),
            Topics = topics,
            KnowledgeStructures = knowledgeStructures,
            Subjects = subjects,
        };

        return View(model);
    }

    [HttpPost("Questions/Edit/{id}")]
    public async Task<IActionResult> Edit(long id, EditQuestionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns
            var (topics, knowledgeStructures, subjects) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);
            model.AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false);
            model.Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false);
            model.QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false);
            model.Topics = topics;
            model.KnowledgeStructures = knowledgeStructures;
            model.Subjects = subjects;
            return View(model);
        }

        var command = new UpdateQuestionCommand(
            Id: id,
            Text: model.Text,
            AnswerType: model.AnswerType,
            AppliesToPhase: model.AppliesToPhase,
            IsUsedForMentoringPlan: model.IsUsedForMentoringPlan,
            IsUsedForDiagnosis: model.IsUsedForDiagnosis,
            AnswerOptions: model.AnswerOptions?.Select(o => new UpdateAnswerOptionDto(
                Id: null,
                Text: o.Text,
                Score: o.Score,
                Foda: o.Foda,
                FodaExplanation: o.FodaExplanation,
                Odsr: o.Odsr,
                OdsrExplanation: o.OdsrExplanation,
                FollowupQuestionText: o.FollowupQuestionText,
                Order: o.Order)).ToList());

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Pregunta actualizada exitosamente.");
            return RedirectToAction(nameof(List));
        }

        this.MapErrorsToModelStateAndSetErrorToast<EditQuestionViewModel>(result);

        // Reload dropdowns
        var (topics2, knowledgeStructures2, subjects2) = await GetKnowledgeStructureHierarchy(cancellationToken).ConfigureAwait(false);
        model.AnswerTypes = await GetAllAnswerTypesAsync(cancellationToken).ConfigureAwait(false);
        model.Blocks = await GetAllBlocksAsync(cancellationToken).ConfigureAwait(false);
        model.QuestionPhases = await GetAllQuestionPhasesAsync(cancellationToken).ConfigureAwait(false);
        model.Topics = topics2;
        model.KnowledgeStructures = knowledgeStructures2;
        model.Subjects = subjects2;

        return View(model);
    }

    [HttpPost("Questions/Delete/{id}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteQuestionCommand(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            this.SetSuccessToast("Pregunta eliminada exitosamente.");
        }
        else
        {
            this.SetErrorToast(result.ErrorMessages?.FirstOrDefault().Message ?? "Error al eliminar la pregunta.");
        }

        return RedirectToAction(nameof(List));
    }

    private Task<Dictionary<int, string>> GetAllAnswerTypesAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllAnswerTypesQuery();
        return MediatorExecutor.SendOrThrowAsync(query, cancellationToken);
    }

    private Task<Dictionary<long, string>> GetAllBlocksAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllBlocksQuery();
        return MediatorExecutor.SendOrThrowAsync(query, cancellationToken);
    }

    private Task<Dictionary<int, string>> GetAllQuestionPhasesAsync(CancellationToken cancellationToken)
    {
        var query = new GetAllQuestionPhasesQuery();
        return MediatorExecutor.SendOrThrowAsync(query, cancellationToken);
    }

    private async Task<(
        Dictionary<long, QuestionListTopicHierarchyViewModel> Topics,
        Dictionary<long, string> KnowledgeStructures,
        Dictionary<long, QuestionListSubjectHierarchyViewModel> Subjects)> GetKnowledgeStructureHierarchy(CancellationToken cancellationToken)
    {
        var query = new GetAllKnowledgeStructuresWithHierarchyQuery();
        var result = await MediatorExecutor.SendOrThrowAsync(query, cancellationToken).ConfigureAwait(false);

        var topics = new Dictionary<long, QuestionListTopicHierarchyViewModel>();
        var knowledgeStructures = new Dictionary<long, string>();
        var subjects = new Dictionary<long, QuestionListSubjectHierarchyViewModel>();

        foreach (var ks in result)
        {
            knowledgeStructures.Add(ks.Id, ks.Name);

            foreach (var module in ks.Modules)
            {
                foreach (var topic in module.Topics)
                {
                    topics.Add(topic.Id, new QuestionListTopicHierarchyViewModel(
                        ks.Name,
                        module.Name,
                        topic.Name));

                    foreach (var subject in topic.Subjects)
                    {
                        subjects.Add(subject.Id, new QuestionListSubjectHierarchyViewModel(
                            ks.Name,
                            module.Name,
                            topic.Name,
                            subject.Title));
                    }
                }
            }
        }

        return (topics, knowledgeStructures, subjects);
    }
}
