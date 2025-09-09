using FluentValidation;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;
using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.GetOrCreateFormSubmission;

/// <summary>
/// Command to get an existing form submission or create a new one for a specific phase.
/// </summary>
public sealed record GetOrCreateFormSubmissionCommand(Guid ProjectExternalId, string UserId, QuestionPhase Phase) : IBaseRequest<FormSubmissionDto>;

/// <summary>
/// Validator for GetOrCreateFormSubmissionCommand.
/// </summary>
public class GetOrCreateFormSubmissionCommandValidator : AbstractValidator<GetOrCreateFormSubmissionCommand>
{
    public GetOrCreateFormSubmissionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.Phase)
            .IsInEnum()
            .NotEqual(QuestionPhase.None)
            .WithMessage("La fase debe ser válida.");
    }
}

/// <summary>
/// Handler for GetOrCreateFormSubmissionCommand.
/// </summary>
public class GetOrCreateFormSubmissionCommandHandler(IBusinessIncubatorRepository businessIncubatorRepository, ITimeProvider timeProvider)
    : BaseCommandHandler<GetOrCreateFormSubmissionCommand, FormSubmissionDto>
{
    public override async Task<Result<FormSubmissionDto>> Handle(GetOrCreateFormSubmissionCommand request, CancellationToken cancellationToken)
    {
        // Get project with stages
        var project = await businessIncubatorRepository.GetProjectWithStagesByExternalIdAsync(
            request.ProjectExternalId,
            cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
        }

        // Check if user is a participant
        var isParticipant = await businessIncubatorRepository.IsUserProjectParticipantAsync(
            project.Id,
            request.UserId,
            cancellationToken);

        if (!isParticipant)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NotParticipant, ("User", "No es participante del proyecto."));
        }

        // Get project knowledge structure
        var knowledgeStructure = await businessIncubatorRepository.GetProjectKnowledgeStructureAsync(
            project.Id,
            cancellationToken);

        if (knowledgeStructure is null)
        {
            return Failure(ResultErrorCodes.Project_KnowledgeStructureNotFound, ("KnowledgeStructure", "El proyecto no tiene una estructura de conocimiento configurada."));
        }

        // Check current stage matches requested phase
        var currentDate = timeProvider.UtcNow;
        var currentStage = project.GetCurrentStage(currentDate);

        if (currentStage is null)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NoActiveStage, ("Stage", "El proyecto no tiene una etapa activa actualmente."));
        }

        // Verify stage is active
        if (!currentStage.IsActive)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_StageNotActive, ("Stage", $"La etapa {currentStage.Title} no está activa."));
        }

        var expectedPhase = ProjectFormSubmission.GetPhaseForStage(currentStage.Type);
        if (expectedPhase != request.Phase && expectedPhase != QuestionPhase.Undefined)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_InvalidPhase, ("Phase", $"La fase solicitada ({request.Phase}) no coincide con la etapa actual del proyecto ({currentStage.Type})."));
        }

        // Validate access window
        if (!currentStage.IsWithinPeriod(currentDate))
        {
            if (currentDate < currentStage.StartDate)
            {
                return Failure(ResultErrorCodes.ProjectFormSubmission_BeforeWindow, ("Window", $"La etapa {currentStage.Title} comienza el {currentStage.StartDate:dd/MM/yyyy}."));
            }
            else
            {
                return Failure(ResultErrorCodes.ProjectFormSubmission_AfterWindow, ("Window", $"La etapa {currentStage.Title} finalizó el {currentStage.EndDate:dd/MM/yyyy}."));
            }
        }

        // Get existing submission or create new
        var submission = await businessIncubatorRepository.GetFormSubmissionAsync(
            project.Id,
            request.UserId,
            request.Phase,
            cancellationToken);

        // Check if already submitted or approved
        if (submission is not null)
        {
            if (submission.Status == ProjectFormSubmissionStatus.Approved)
            {
                return Failure(ResultErrorCodes.ProjectFormSubmission_AlreadyApproved, ("Status", "El formulario ya fue aprobado y no puede ser modificado."));
            }

            if (submission.Status == ProjectFormSubmissionStatus.Submitted)
            {
                return Failure(ResultErrorCodes.ProjectFormSubmission_AlreadySubmitted, ("Status", "El formulario ya fue enviado y está pendiente de revisión."));
            }
        }

        if (submission is null)
        {
            // Create new submission
            submission = ProjectFormSubmission.CreateForPhase(
                project.Id,
                request.UserId,
                knowledgeStructure.CurrentVersion,
                request.Phase,
                currentStage?.Id,
                currentDate);

            businessIncubatorRepository.AddFormSubmission(submission);
            await businessIncubatorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Map to DTO
        var dto = new FormSubmissionDto
        {
            Id = submission.Id,
            ExternalId = submission.ExternalId,
            ProjectId = submission.ProjectId,
            ParticipantUserId = submission.ParticipantUserId,
            FormSchemaVersion = submission.FormSchemaVersion,
            StatusEnum = submission.Status,
            Status = submission.Status.ToString(),
            StatusCode = (int)submission.Status,
            StartedAt = submission.StartedAt,
            SubmittedAt = submission.SubmittedAt,
            ApprovedAt = submission.ApprovedAt,
            ApprovedByUserId = submission.ApprovedByUserId,
            RejectionReason = submission.RejectionReason,
            RejectedAt = submission.RejectedAt,
            Phase = submission.Phase,
            ProjectStageId = submission.ProjectStageId,
            CompletionPercentage = submission.CompletionPercentage,
            LastAutoSaveAt = submission.LastAutoSaveAt,
            TotalQuestions = submission.TotalQuestions,
            AnsweredQuestions = submission.AnsweredQuestions,
            CanEdit = submission.Status is ProjectFormSubmissionStatus.Draft or ProjectFormSubmissionStatus.Rejected,
            CanSubmit = submission.Status == ProjectFormSubmissionStatus.Draft && !string.IsNullOrWhiteSpace(submission.DraftData)
        };

        return Success(dto);
    }
}
