using System.Text.Json;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Services;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;

/// <summary>
/// Handler for getting form submission details.
/// </summary>
public sealed class GetFormSubmissionQueryHandler(
    IBusinessIncubatorRepository repository,
    IDraftDataAdapter draftDataAdapter,
    ILogger<GetFormSubmissionQueryHandler> logger) : BaseCommandHandler<GetFormSubmissionQuery, FormSubmissionDto>
{
    public override async Task<Result<FormSubmissionDto>> Handle(GetFormSubmissionQuery request, CancellationToken cancellationToken)
    {
        // Get project
        var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "El proyecto no existe."));
        }

        // Get project with form submissions
        project = await repository.GetProjectWithFormSubmissionsAsync(project.Id, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Unknown, ("LoadProject", "Error al cargar el proyecto."));
        }

        // Load knowledge structure if not already loaded
        if (!project.HasKnowledgeStructure())
        {
            project = await repository.GetProjectWithKnowledgeStructureByIdAsync(project.Id, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Unknown, ("LoadKnowledgeStructure", "Error al cargar la estructura del proyecto."));
            }
        }

        // Check if participant has access
        if (!project.HasFormAccess(request.ParticipantUserId))
        {
            return Failure(ResultErrorCodes.Auth_UserHasNoAccessToProtectedResource, (nameof(request.ParticipantUserId), "No tienes acceso a este proyecto."));
        }

        // Find the submission
        var submission = project.FormSubmissions
            .FirstOrDefault(s => s.ParticipantUserId == request.ParticipantUserId && s.FormId == request.FormId);

        if (submission is null)
        {
            // No submission exists yet, return empty result indicating they can start
            return Success(new FormSubmissionDto
            {
                ProjectId = project.Id,
                FormId = request.FormId,
                ParticipantUserId = request.ParticipantUserId,
                Status = "No Iniciado",
                StatusCode = 0,
                CanEdit = true,
                CanSubmit = false
            });
        }

        // Parse draft data if exists
        DraftDataDto? draftData = null;
        if (!string.IsNullOrWhiteSpace(submission.DraftData))
        {
            try
            {
                draftData = JsonSerializer.Deserialize<DraftDataDto>(submission.DraftData);

                // Check if draft needs adaptation to current form version
                if (draftData is not null && project.HasKnowledgeStructure())
                {
                    var currentVersion = project.GetCurrentKnowledgeStructureVersion();
                    if (draftDataAdapter.IsAdaptationNeeded(submission.FormSchemaVersion, currentVersion))
                    {
                        logger.LogInformation(
                            "Adapting draft data for submission {SubmissionId} from version {FromVersion} to {ToVersion}",
                            submission.Id,
                            submission.FormSchemaVersion,
                            currentVersion);

                        // Adapt the draft data to the current form structure
                        var knowledgeStructure = project.GetKnowledgeStructure();
                        if (knowledgeStructure is not null)
                        {
                            draftData = await draftDataAdapter.AdaptToCurrentVersionAsync(
                                draftData,
                                submission.FormSchemaVersion,
                                currentVersion,
                                knowledgeStructure);
                        }

                        // Note: We don't update the submission here as this is a query
                        // The adapted data is only for display purposes
                        // The original draft data remains unchanged in the database
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                logger.LogError(ex, "Error parsing or adapting draft data for submission {SubmissionId}", submission.Id);
                draftData = null;
            }
        }

        var dto = new FormSubmissionDto
        {
            Id = submission.Id,
            ProjectId = submission.ProjectId,
            FormId = submission.FormId,
            ParticipantUserId = submission.ParticipantUserId,
            Status = GetStatusDisplay(submission.Status),
            StatusCode = (int)submission.Status,
            StartedAt = submission.StartedAt,
            SubmittedAt = submission.SubmittedAt,
            ApprovedAt = submission.ApprovedAt,
            RejectionReason = submission.RejectionReason,
            DraftData = draftData,
            CanEdit = submission.Status == ProjectFormSubmissionStatus.Draft ||
                      submission.Status == ProjectFormSubmissionStatus.Rejected,
            CanSubmit = submission.Status == ProjectFormSubmissionStatus.Draft &&
                        !string.IsNullOrWhiteSpace(submission.DraftData)
        };

        return Success(dto);
    }

    private static string GetStatusDisplay(ProjectFormSubmissionStatus status)
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
}