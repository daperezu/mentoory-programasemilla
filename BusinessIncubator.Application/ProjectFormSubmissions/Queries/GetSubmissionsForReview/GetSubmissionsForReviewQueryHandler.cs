using System.Text.Json;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetSubmissionsForReview;

/// <summary>
/// Handler for getting submissions for review.
/// </summary>
public sealed class GetSubmissionsForReviewQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetSubmissionsForReviewQuery, List<SubmissionForReviewDto>>
{
    public override async Task<Result<List<SubmissionForReviewDto>>> Handle(GetSubmissionsForReviewQuery request, CancellationToken cancellationToken)
    {
        // Get project
        var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "El proyecto no existe."));
        }

        // Get project with form submissions and invitations
        project = await repository.GetProjectWithFormSubmissionsAsync(project.Id, cancellationToken);
        if (project is null)
        {
            return Failure(ResultErrorCodes.Unknown, ("LoadProject", "Error al cargar el proyecto."));
        }

        // Filter submissions based on request
        var submissions = request.OnlyPending
            ? project.FormSubmissions.Where(s => s.Status == ProjectFormSubmissionStatus.Submitted)
            : project.FormSubmissions;

        var dtos = new List<SubmissionForReviewDto>();

        foreach (var submission in submissions)
        {
            // Try to get participant info from invitations
            var invitation = project.ProjectInvitations
                .FirstOrDefault(i => i.IdentificationNumber == submission.ParticipantUserId.ToString());

            // Parse draft to get completion percentage
            decimal completionPercentage = 0;
            if (!string.IsNullOrWhiteSpace(submission.DraftData))
            {
                try
                {
                    var draftData = JsonSerializer.Deserialize<DraftDataDto>(submission.DraftData);
                    completionPercentage = draftData?.ProgressPercentage ?? 0;
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            var dto = new SubmissionForReviewDto
            {
                Id = submission.Id,
                ParticipantUserId = submission.ParticipantUserId,
                ParticipantName = invitation?.FullName ?? "Desconocido",
                ParticipantEmail = invitation?.Email ?? string.Empty,
                FormId = submission.FormId,
                FormName = $"Formulario {submission.FormId}", // TODO: Get actual form name
                Status = GetStatusDisplay(submission.Status),
                StatusCode = (int)submission.Status,
                StartedAt = submission.StartedAt,
                SubmittedAt = submission.SubmittedAt,
                ReviewedAt = submission.ApprovedAt,
                CompletionPercentage = completionPercentage
            };

            dtos.Add(dto);
        }

        // Sort by submission date (newest first)
        dtos = dtos.OrderByDescending(d => d.SubmittedAt ?? d.StartedAt).ToList();

        return Success(dtos);
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