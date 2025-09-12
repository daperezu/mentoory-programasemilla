using LinaSys.Auth.Domain.Repositories;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.Orchestration.Application.Diagnostics.Queries;

/// <summary>
/// Handler for fetching presentation data for diagnosis charts.
/// This is an orchestration handler that coordinates between multiple domains.
/// </summary>
public class GetDiagnosisChartPresentationDataQueryHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    IAuthRepository authRepository,
    ILogger<GetDiagnosisChartPresentationDataQueryHandler> logger) : BaseCommandHandler<GetDiagnosisChartPresentationDataQuery, DiagnosisChartPresentationDto>
{
    public override async Task<Result<DiagnosisChartPresentationDto>> Handle(
        GetDiagnosisChartPresentationDataQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch project and incubator data
            var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, ("project", "Proyecto no encontrado"));
            }

            var incubator = await businessIncubatorRepository.GetByIdAsync(project.BusinessIncubatorId, cancellationToken);

            // Fetch participant data
            var participant = await authRepository.FindUserByIdAsync(request.ParticipantUserId, cancellationToken);

            var dto = new DiagnosisChartPresentationDto
            {
                IncubatorName = incubator?.Name ?? "Incubadora",
                ProjectName = project.Name,
                ParticipantName = participant?.Email ?? participant?.UserName ?? "Participante"
            };

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error fetching presentation data for project {ProjectExternalId}, user {UserId}",
                request.ProjectExternalId, request.ParticipantUserId);

            return Failure(ResultErrorCodes.GenericError, ("error", "Error al obtener los datos de presentación"));
        }
    }
}