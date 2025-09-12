using LinaSys.Diagnostics.Application.Charts.DTOs;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Charts.Queries;

/// <summary>
/// Query to fetch and aggregate chart data for diagnosis review.
/// </summary>
public record GetDiagnosisChartDataQuery(
    long ProjectId,
    string ParticipantUserId,
    QuestionPhase Phase) : IBaseRequest<DiagnosisReviewDto>;