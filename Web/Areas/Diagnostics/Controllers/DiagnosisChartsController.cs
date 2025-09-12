using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Orchestration.Application.Diagnostics.Queries;
using Microsoft.Extensions.Caching.Memory;
using LinaSys.Web.Areas.Diagnostics.Models;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Diagnostics.Controllers;

/// <summary>
/// Controller for managing diagnosis charts visualization.
/// </summary>
[Area("Diagnostics")]
[Route("[area]/[controller]/[action]")]
[Authorize(Roles = "Coordinator")]
public class DiagnosisChartsController(
    ILogger<DiagnosisChartsController> logger,
    MediatorExecutor mediatorExecutor,
    IApplicationUrlService applicationUrlService,
    IMemoryCache cache) : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{

    /// <summary>
    /// Shows the diagnosis review charts for a participant.
    /// </summary>
    /// <param name="projectExternalId">The project external identifier.</param>
    /// <param name="participantUserId">The participant user identifier.</param>
    /// <param name="phase">The question phase.</param>
    /// <returns>The review view with charts.</returns>
    [HttpGet("{projectExternalId:guid}/{participantUserId}/{phase:int}")]
    public async Task<IActionResult> Review(Guid projectExternalId, string participantUserId, int phase)
    {
        try
        {
            // Access is already verified by the Authorize attribute
            var questionPhase = (QuestionPhase)phase;

            // Check cache first
            var cacheKey = $"diagnosis-chart:{projectExternalId}:{participantUserId}:{questionPhase}";
            var cachedData = cache.Get<LinaSys.Diagnostics.Application.Charts.DTOs.DiagnosisReviewDto>(cacheKey);

            if (cachedData != null)
            {
                // Fetch presentation data for cached results
                var cachedPresentationQuery = new GetDiagnosisChartPresentationDataQuery(projectExternalId, participantUserId);
                var cachedPresentationResult = await MediatorExecutor.SendAndLogIfFailureAsync(cachedPresentationQuery);

                if (!cachedPresentationResult.IsSuccess)
                {
                    MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(cachedPresentationResult);
                    return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
                }

                var cachedViewModel = MapToViewModel(
                    cachedData,
                    projectExternalId,
                    participantUserId,
                    phase,
                    cachedPresentationResult.Value!.IncubatorName,
                    cachedPresentationResult.Value.ProjectName,
                    cachedPresentationResult.Value.ParticipantName);
                return View(cachedViewModel);
            }

            // Execute orchestration query to get chart data (handles project resolution internally)
            var query = new GetDiagnosisChartDataOrchestrationQuery(projectExternalId, participantUserId, questionPhase);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(result);
                return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
            }

            // Fetch presentation data
            var presentationQuery = new GetDiagnosisChartPresentationDataQuery(projectExternalId, participantUserId);
            var presentationResult = await MediatorExecutor.SendAndLogIfFailureAsync(presentationQuery);

            if (!presentationResult.IsSuccess)
            {
                MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(presentationResult);
                return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
            }

            // Cache the data for 5 minutes
            cache.Set(cacheKey, result.Value!, TimeSpan.FromMinutes(5));

            var viewModel = MapToViewModel(
                result.Value!,
                projectExternalId,
                participantUserId,
                phase,
                presentationResult.Value!.IncubatorName,
                presentationResult.Value.ProjectName,
                presentationResult.Value.ParticipantName);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error loading diagnosis charts for project {ProjectExternalId}, user {UserId}, phase {Phase}",
                projectExternalId, participantUserId, phase);

            this.SetErrorToast("Error al cargar los gráficos de diagnóstico");
            return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
        }
    }

    /// <summary>
    /// Shows the print-friendly version of diagnosis charts.
    /// </summary>
    /// <param name="projectExternalId">The project external identifier.</param>
    /// <param name="participantUserId">The participant user identifier.</param>
    /// <param name="phase">The question phase.</param>
    /// <returns>The print view with charts.</returns>
    [HttpGet("{projectExternalId:guid}/{participantUserId}/{phase:int}")]
    public async Task<IActionResult> Print(Guid projectExternalId, string participantUserId, int phase)
    {
        try
        {
            // Access is already verified by the Authorize attribute
            var questionPhase = (QuestionPhase)phase;

            // Check cache first
            var cacheKey = $"diagnosis-chart:{projectExternalId}:{participantUserId}:{questionPhase}";
            var cachedData = cache.Get<LinaSys.Diagnostics.Application.Charts.DTOs.DiagnosisReviewDto>(cacheKey);

            if (cachedData != null)
            {
                // Fetch presentation data for cached results
                var cachedPresentationQuery = new GetDiagnosisChartPresentationDataQuery(projectExternalId, participantUserId);
                var cachedPresentationResult = await MediatorExecutor.SendAndLogIfFailureAsync(cachedPresentationQuery);

                if (!cachedPresentationResult.IsSuccess)
                {
                    MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(cachedPresentationResult);
                    return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
                }

                var cachedViewModel = MapToViewModel(
                    cachedData,
                    projectExternalId,
                    participantUserId,
                    phase,
                    cachedPresentationResult.Value!.IncubatorName,
                    cachedPresentationResult.Value.ProjectName,
                    cachedPresentationResult.Value.ParticipantName);
                return View(cachedViewModel);
            }

            // Execute orchestration query to get chart data (handles project resolution internally)
            var query = new GetDiagnosisChartDataOrchestrationQuery(projectExternalId, participantUserId, questionPhase);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);

            if (!result.IsSuccess)
            {
                MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(result);
                return RedirectToAction("Review", new { projectExternalId, participantUserId, phase });
            }

            // Fetch presentation data
            var presentationQuery = new GetDiagnosisChartPresentationDataQuery(projectExternalId, participantUserId);
            var presentationResult = await MediatorExecutor.SendAndLogIfFailureAsync(presentationQuery);

            if (!presentationResult.IsSuccess)
            {
                MapErrorsToModelStateAndSetErrorToast<DiagnosisReviewViewModel>(presentationResult);
                return RedirectToAction("Index", "Dashboard", new { area = string.Empty });
            }

            // Cache the data for 5 minutes
            cache.Set(cacheKey, result.Value!, TimeSpan.FromMinutes(5));

            var viewModel = MapToViewModel(
                result.Value!,
                projectExternalId,
                participantUserId,
                phase,
                presentationResult.Value!.IncubatorName,
                presentationResult.Value.ProjectName,
                presentationResult.Value.ParticipantName);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error loading print view for project {ProjectExternalId}, user {UserId}, phase {Phase}",
                projectExternalId, participantUserId, phase);

            this.SetErrorToast("Error al cargar la vista de impresión");
            return RedirectToAction("Review", new { projectExternalId, participantUserId, phase });
        }
    }

    private DiagnosisReviewViewModel MapToViewModel(
        LinaSys.Diagnostics.Application.Charts.DTOs.DiagnosisReviewDto dto,
        Guid projectExternalId,
        string participantUserId,
        int phase,
        string incubatorName,
        string projectName,
        string participantName)
    {
        var phaseDisplay = ((QuestionPhase)phase) == QuestionPhase.Start ? "Inicio" : "Final";

        var viewModel = new DiagnosisReviewViewModel
        {
            IncubatorName = incubatorName,
            ProjectName = projectName,
            ParticipantName = participantName,
            PhaseDisplay = phaseDisplay,
            ReviewDate = dto.SubmissionDate,
            PrintUrl = Url.Action("Print", new { projectExternalId, participantUserId, phase }),
            Charts = dto.Charts.Select(chart => new ChartViewModel
            {
                BlockId = chart.BlockId.ToString(),
                BlockName = chart.BlockName,
                ChartElementId = $"chart-block-{chart.BlockId}",
                ChartDataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    blockName = chart.BlockName,
                    maxScore = chart.MaxScore,
                    labels = chart.Scores.Select(s => s.Label).ToArray(),
                    scores = chart.Scores.Select(s => s.Value).ToArray(),
                    questions = chart.Scores.Select(s => new
                    {
                        label = s.Label,
                        text = s.QuestionText,
                        score = s.Value,
                        source = s.Source
                    }).ToArray()
                })
            }).ToList()
        };

        return viewModel;
    }
}