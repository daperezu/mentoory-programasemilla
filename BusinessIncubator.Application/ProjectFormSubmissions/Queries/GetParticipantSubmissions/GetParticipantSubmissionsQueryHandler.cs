using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetParticipantSubmissions;

/// <summary>
/// Handler for getting all form submissions for a specific participant.
/// </summary>
public sealed class GetParticipantSubmissionsQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetParticipantSubmissionsQueryHandler> logger) : BaseCommandHandler<GetParticipantSubmissionsQuery, List<ParticipantSubmissionDto>>
{
    public override async Task<Result<List<ParticipantSubmissionDto>>> Handle(
        GetParticipantSubmissionsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // First get project by external ID
            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(
                    ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "El proyecto no existe."));
            }

            // Get project with form submissions
            var projectWithSubmissions = await repository.GetProjectWithFormSubmissionsAsync(project.Id, cancellationToken);
            if (projectWithSubmissions is null)
            {
                return Failure(
                    ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "El proyecto no existe."));
            }

            // Get participant's submissions
            var participantSubmissions = projectWithSubmissions.FormSubmissions
                .Where(s => s.ParticipantUserId == request.ParticipantUserId)
                .OrderByDescending(s => s.StartedAt)
                .ToList();

            // Map to DTOs
            var submissions = participantSubmissions.Select(submission => new ParticipantSubmissionDto
            {
                Id = submission.Id,
                FormName = "Formulario de Diagnóstico", // Generic name for now
                Status = GetStatusText(submission.Status),
                StatusBadgeClass = GetStatusBadgeClass(submission.Status),
                SubmittedAt = submission.SubmittedAt,
                ReviewedAt = submission.Status == ProjectFormSubmissionStatus.Approved ? submission.ApprovedAt : null,
                RejectionReason = submission.RejectionReason,
                CanEdit = submission.Status == ProjectFormSubmissionStatus.Draft ||
                         submission.Status == ProjectFormSubmissionStatus.Rejected,
                CanView = submission.Status == ProjectFormSubmissionStatus.Submitted ||
                         submission.Status == ProjectFormSubmissionStatus.Approved
            }).ToList();

            logger.LogInformation(
                "Retrieved {Count} submissions for participant {ParticipantUserId} in project {ProjectId}",
                submissions.Count,
                request.ParticipantUserId,
                request.ProjectExternalId);

            return Success(submissions);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error getting submissions for participant {ParticipantUserId} in project {ProjectId}",
                request.ParticipantUserId,
                request.ProjectExternalId);

            return Failure(
                ResultErrorCodes.Unknown,
                ("GetSubmissions", "Error al obtener los formularios. Por favor, intenta nuevamente."));
        }
    }

    private static string GetStatusText(ProjectFormSubmissionStatus status)
    {
        return status switch
        {
            ProjectFormSubmissionStatus.Draft => "Borrador",
            ProjectFormSubmissionStatus.Submitted => "Enviado",
            ProjectFormSubmissionStatus.Approved => "Aprobado",
            ProjectFormSubmissionStatus.Rejected => "Rechazado",
            _ => "Desconocido"
        };
    }

    private static string GetStatusBadgeClass(ProjectFormSubmissionStatus status)
    {
        return status switch
        {
            ProjectFormSubmissionStatus.Draft => "secondary",
            ProjectFormSubmissionStatus.Submitted => "primary",
            ProjectFormSubmissionStatus.Approved => "success",
            ProjectFormSubmissionStatus.Rejected => "warning",
            _ => "secondary"
        };
    }
}