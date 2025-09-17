using LinaSys.Shared.Application.MediatR;
using LinaSys.UserProjectDiagnosis.Domain.Aggregates.FormStructureBlock;

namespace LinaSys.UserProjectDiagnosis.Application.Queries.GetDiagnosisChartData;

/// <summary>
/// Query to fetch and aggregate chart data for diagnosis review.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="ParticipantUserId">The participant user identifier.</param>
/// <param name="Phase">The question phase.</param>
public record GetDiagnosisChartDataQuery(
    long ProjectId,
    string ParticipantUserId,
    QuestionPhase Phase) : IBaseRequest<DiagnosisReviewDto>;