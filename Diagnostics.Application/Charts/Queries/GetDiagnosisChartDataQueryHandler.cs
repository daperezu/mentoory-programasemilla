using LinaSys.Diagnostics.Application.Charts.DTOs;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Diagnostics.Domain.Services;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Diagnostics.Application.Charts.Queries;

/// <summary>
/// Handler for fetching and aggregating diagnosis chart data.
/// </summary>
public class GetDiagnosisChartDataQueryHandler(
    IUserProjectDiagnosisRepository diagnosisRepository,
    DiagnosisScoreCalculator scoreCalculator,
    ILogger<GetDiagnosisChartDataQueryHandler> logger) : BaseCommandHandler<GetDiagnosisChartDataQuery, DiagnosisReviewDto>
{
    public override async Task<Result<DiagnosisReviewDto>> Handle(
        GetDiagnosisChartDataQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch all diagnosis answers for project/user/phase
            var answers = await diagnosisRepository.GetApprovedDiagnosisAnswersAsync(
                request.ProjectId,
                request.ParticipantUserId,
                request.Phase,
                cancellationToken);

            var answersList = answers.ToList();

            if (!answersList.Any())
            {
                return Failure(ResultErrorCodes.Project_NotFound, ("diagnosis", "No se encontraron respuestas de diagnóstico aprobadas"));
            }

            // 4. Get blocks with questions
            var blocks = await diagnosisRepository.GetBlocksWithQuestionsAsync(
                request.ProjectId,
                cancellationToken);

            // 5. Group answers by BlockId
            var answersByBlock = answersList.GroupBy(a => a.BlockId);

            // 6. For each block, create chart data
            var charts = new List<DiagnosisChartDto>();
            foreach (var blockGroup in answersByBlock)
            {
                var blockId = blockGroup.Key;
                var blockAnswers = blockGroup.ToList();
                // Get block name from answers or blocks list
                var blockName = blockAnswers.FirstOrDefault()?.BlockName
                    ?? blocks.FirstOrDefault(b => b.BlockId == blockId).BlockName
                    ?? $"Bloque {blockId}";

                // Build chart data using domain service
                var blockChartData = scoreCalculator.BuildBlockChartData(
                    blockId,
                    blockName,
                    blockAnswers,
                    maxScore: 10);

                // Map to DTO
                var chartDto = new DiagnosisChartDto
                {
                    BlockId = blockChartData.BlockId,
                    BlockName = blockChartData.BlockName,
                    MaxScore = blockChartData.MaxScore,
                    Scores = blockChartData.Scores.Select(s => new QuestionScoreDto
                    {
                        Label = s.Label,
                        Value = s.Score,
                        QuestionText = blockAnswers
                            .FirstOrDefault(a => a.QuestionId == s.QuestionId)?.QuestionText
                            ?? string.Empty,
                        Source = s.Source
                    }).ToList()
                };

                charts.Add(chartDto);
            }

            // 7. Create review DTO with only diagnosis-specific data
            var reviewDto = new DiagnosisReviewDto
            {
                SubmissionDate = answersList.FirstOrDefault()?.SubmittedAt ?? DateTime.UtcNow,
                Phase = request.Phase,
                Charts = charts.OrderBy(c => c.BlockId).ToList()
            };

            return Success(reviewDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error fetching diagnosis chart data for project {ProjectId}, user {UserId}, phase {Phase}",
                request.ProjectId, request.ParticipantUserId, request.Phase);

            return Failure(ResultErrorCodes.GenericError, ("error", "Error al obtener los datos del diagnóstico"));
        }
    }
}