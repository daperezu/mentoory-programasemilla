using LinaSys.Orchestration.Application.Diagnostics.Commands;
using LinaSys.Orchestration.Application.Diagnostics.Queries;
using LinaSys.Web.Areas.Diagnostics.Models.DiagnosisForms;
using LinaSys.Web.Controllers;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Diagnostics.Controllers;

[Area("Diagnostics")]
[Route("Diagnostics")]
public class DiagnosisFormsController(ILogger<DiagnosisFormsController> logger, MediatorExecutor mediatorExecutor, IApplicationUrlService applicationUrlService) : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{
    [HttpGet("DiagnosisForms/{id:guid}/{phase}")]
    public async Task<IActionResult> Index(Guid id, QuestionPhase phase, CancellationToken cancellationToken)
    {
        var questionBlocks = await GetQuestionBlocksAsync(id, phase, cancellationToken);

        ViewBag.FormId = id;
        ViewBag.Phase = phase;

        var model = new DiagnosisFormViewModel
        {
            QuestionBlocks = questionBlocks,
        };
        return View(model);
    }

    [HttpPost("DiagnosisForms/{id:guid}/{phase}")]
    public async Task<IActionResult> Index(Guid id, QuestionPhase phase, [FromForm] List<AnswerViewModel> answers, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid && answers.Any())
        {
            var command = new UpsertAnswersToProjectFormCommand(
                id,
                (int)phase,
                answers.Select(a => new ProjectAnswerDto(
                    a.QuestionId,
                    a.AnswerOptionId,
                    a.UserInput,
                    a.FollowUpUserInput)).ToList());

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Answers saved successfully.";
                return RedirectToAction(nameof(Index), new { id, phase });
            }

            // Add errors to ModelState
            if (result.ErrorMessages is not null)
            {
                foreach (var error in result.ErrorMessages)
                {
                    ModelState.AddModelError(error.Context ?? string.Empty, error.Message);
                }
            }
        }

        var questionBlocks = await GetQuestionBlocksAsync(id, phase, cancellationToken);

        ViewBag.FormId = id;
        ViewBag.Phase = phase;
        var model = new DiagnosisFormViewModel
        {
            QuestionBlocks = questionBlocks,
        };
        return View(model);
    }

    private async Task<List<BlockViewModel>> GetQuestionBlocksAsync(Guid externalId, QuestionPhase phase, CancellationToken cancellationToken)
    {
        var query = new GetProjectFormByExternalIdForDiagnosisQuery(externalId, (int)phase);
        var result = await MediatorExecutor.SendOrThrowAsync(query, cancellationToken);

        return result.Select(block => new BlockViewModel(
            block.Id,
            block.Title,
            block.Questions.Select(q => new QuestionViewModel(
                q.Id,
                (AnswerTypeViewModel)q.AnswerType,
                q.Text,
                q.Overriden,
                IsRequired: false,
                q.Options.Select(o => new AnswerOptionViewModel(
                    o.Id,
                    o.Text,
                    o.Overriden,
                    o.FollowUpQuestionText,
                    false)).ToList())).ToList())).ToList();
    }
}
