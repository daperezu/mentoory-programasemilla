using LinaSys.Auth.Application.Queries.GetUsersByIds;
using LinaSys.BusinessIncubator.Application.Queries.GetProject;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserProjectDiagnosis.Domain.Aggregates.DiagnosisAnswer;
using LinaSys.UserProjectDiagnosis.Domain.Repositories;
using LinaSys.UserProjectDiagnosis.Domain.Services;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinaSys.UserProjectDiagnosis.Application.Queries.GetDiagnosisChartData;

/// <summary>
/// Handler for GetDiagnosisChartDataQuery.
/// </summary>
public class GetDiagnosisChartDataQueryHandler(
    IUserProjectDiagnosisRepository repository,
    IMediator mediator,
    IMemoryCache cache,
    DiagnosisScoreCalculator scoreCalculator,
    ILogger<GetDiagnosisChartDataQueryHandler> logger)
    : BaseCommandHandler<GetDiagnosisChartDataQuery, DiagnosisReviewDto>
{
    /// <inheritdoc/>
    public override async Task<Result<DiagnosisReviewDto>> Handle(
        GetDiagnosisChartDataQuery request,
        CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cacheKey = $"diagnosis_charts_{request.ProjectId}_{request.ParticipantUserId}_{request.Phase}";
        if (cache.TryGetValue<DiagnosisReviewDto>(cacheKey, out var cachedResult))
        {
            logger.LogDebug(
                "Returning cached chart data for project {ProjectId}, user {UserId}",
                request.ProjectId,
                request.ParticipantUserId);
            return Success(cachedResult!);
        }

        // Get approved diagnosis answers
        var answers = await repository.GetApprovedDiagnosisAnswersAsync(
            request.ProjectId,
            request.ParticipantUserId,
            request.Phase,
            cancellationToken);

        if (!answers.Any())
        {
            return Failure(
                ResultErrorCodes.UserProjectDiagnosis_NotFound,
                (nameof(GetDiagnosisChartDataQuery), "No se encontraron respuestas aprobadas para el diagnóstico."));
        }

        // Get blocks with questions
        var blocks = await repository.GetBlocksWithQuestionsAsync(
            request.ProjectId,
            cancellationToken);

        // Get project information
        var projectResult = await mediator.Send(
            new GetProjectQuery(request.ProjectId),
            cancellationToken);

        if (!projectResult.IsSuccess || projectResult.Value == null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(GetDiagnosisChartDataQuery), "Proyecto no encontrado."));
        }

        // Get user information
        var userResult = await mediator.Send(
            new GetUsersByIdsQuery(new[] { request.ParticipantUserId }),
            cancellationToken);

        var userName = "Participante";
        if (userResult.IsSuccess && userResult.Value != null && userResult.Value.TryGetValue(request.ParticipantUserId, out var userInfo))
        {
            userName = $"{userInfo.FirstName} {userInfo.LastName}";
        }

        // Build charts
        var charts = BuildCharts(answers, blocks);

        // Create result DTO
        var result = new DiagnosisReviewDto
        {
            IncubatorName = projectResult.Value.IncubatorName,
            ProjectName = projectResult.Value.Name,
            ParticipantName = userName,
            ApprovalDate = answers.Max(a => a.UpdatedAt ?? a.CreatedAt),
            Phase = request.Phase,
            Charts = charts,
        };

        // Cache the result for 5 minutes
        cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        logger.LogInformation(
            "Chart data generated for project {ProjectId}, user {UserId}, phase {Phase}",
            request.ProjectId,
            request.ParticipantUserId,
            request.Phase);

        return Success(result);
    }

    private List<DiagnosisChartDto> BuildCharts(
        IEnumerable<DiagnosisAnswer> answers,
        IEnumerable<Domain.DTOs.BlockWithQuestionsDto> blocks)
    {
        var charts = new List<DiagnosisChartDto>();
        var answersByBlock = answers.GroupBy(a => a.BlockId);

        foreach (var blockGroup in answersByBlock)
        {
            var block = blocks.FirstOrDefault(b => b.BlockId == blockGroup.Key);
            if (block == null)
            {
                continue;
            }

            var answersByQuestion = blockGroup.GroupBy(a => a.QuestionId);
            var questionScores = scoreCalculator.CreateQuestionScores(answersByQuestion, blockGroup.Key);

            var chartDto = new DiagnosisChartDto
            {
                BlockId = blockGroup.Key,
                BlockName = block.BlockName,
                Scores = questionScores.Select(qs => new QuestionScoreDto
                {
                    Label = qs.Label,
                    Value = qs.Score,
                    QuestionText = block.Questions.FirstOrDefault(q => q.QuestionId == qs.QuestionId)?.QuestionText ?? string.Empty,
                    Source = qs.Source,
                }).ToList(),
                MaxScore = 10, // Default max score, can be made configurable
            };

            charts.Add(chartDto);
        }

        return charts.OrderBy(c => c.BlockId).ToList();
    }
}