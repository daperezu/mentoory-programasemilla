using LinaSys.BusinessIncubator.Application.Projects.Queries.GetProjectByExternalId;
using LinaSys.Diagnostics.Application.Charts.DTOs;
using LinaSys.Diagnostics.Application.Charts.Queries;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.Diagnostics.Queries;

/// <summary>
/// Orchestration query to fetch diagnosis chart data, handling cross-domain coordination.
/// </summary>
public record GetDiagnosisChartDataOrchestrationQuery(
    Guid ProjectExternalId,
    string ParticipantUserId,
    QuestionPhase Phase) : IBaseRequest<DiagnosisReviewDto>;

/// <summary>
/// Handler for GetDiagnosisChartDataOrchestrationQuery.
/// Orchestrates between BusinessIncubator and Diagnostics domains.
/// </summary>
public class GetDiagnosisChartDataOrchestrationQueryHandler(
    IMediator mediator,
    ILogger<GetDiagnosisChartDataOrchestrationQueryHandler> logger)
    : BaseCommandHandler<GetDiagnosisChartDataOrchestrationQuery, DiagnosisReviewDto>
{
    public override async Task<Result<DiagnosisReviewDto>> Handle(
        GetDiagnosisChartDataOrchestrationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Resolve the project external ID to internal ID using the BusinessIncubator query
            var projectQuery = new GetProjectByExternalIdQuery(request.ProjectExternalId);
            var projectResult = await mediator.Send(projectQuery, cancellationToken);

            if (!projectResult.IsSuccess)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    ("project", "El proyecto no fue encontrado"));
            }

            // Step 2: Call the Diagnostics domain query with the internal ID
            var diagnosisQuery = new GetDiagnosisChartDataQuery(
                projectResult.Value!.Id,
                request.ParticipantUserId,
                request.Phase);

            var result = await mediator.Send(diagnosisQuery, cancellationToken);

            // Return the result from the Diagnostics domain
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error orchestrating diagnosis chart data for project {ProjectExternalId}, user {UserId}, phase {Phase}",
                request.ProjectExternalId, request.ParticipantUserId, request.Phase);

            return Failure(ResultErrorCodes.GenericError,
                ("error", "Error al orquestar los datos del diagnóstico"));
        }
    }
}