using FluentValidation;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmission;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetFormSubmissionByExternalId;

/// <summary>
/// Query to get a form submission by its external ID.
/// </summary>
public sealed record GetFormSubmissionByExternalIdQuery(Guid ExternalId) : IBaseRequest<FormSubmissionDto>;

/// <summary>
/// Validator for GetFormSubmissionByExternalIdQuery.
/// </summary>
public class GetFormSubmissionByExternalIdQueryValidator : AbstractValidator<GetFormSubmissionByExternalIdQuery>
{
    public GetFormSubmissionByExternalIdQueryValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("El ID externo del formulario es requerido.");
    }
}

/// <summary>
/// Handler for GetFormSubmissionByExternalIdQuery.
/// </summary>
public class GetFormSubmissionByExternalIdQueryHandler(
    IBusinessIncubatorRepository repository)
    : BaseCommandHandler<GetFormSubmissionByExternalIdQuery, FormSubmissionDto>
{
    public override async Task<Result<FormSubmissionDto>> Handle(
        GetFormSubmissionByExternalIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get submission with details
        var submission = await repository.GetFormSubmissionWithDetailsByExternalIdAsync(request.ExternalId, cancellationToken);

        if (submission is null)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NotFound, (nameof(request.ExternalId), "El formulario no fue encontrado."));
        }

        // Map to DTO
        var dto = new FormSubmissionDto
        {
            Id = submission.Id,
            ExternalId = submission.ExternalId,
            ProjectId = submission.ProjectId,
            ParticipantUserId = submission.ParticipantUserId,
            FormId = submission.FormId,
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
            CanEdit = submission.Status is Domain.Enums.ProjectFormSubmissionStatus.Draft or Domain.Enums.ProjectFormSubmissionStatus.Rejected,
            CanSubmit = submission.Status == Domain.Enums.ProjectFormSubmissionStatus.Draft && !string.IsNullOrWhiteSpace(submission.DraftData),
            DraftData = !string.IsNullOrWhiteSpace(submission.DraftData)
                ? System.Text.Json.JsonSerializer.Deserialize<DTOs.DraftDataDto>(submission.DraftData)
                : null
        };

        return Success(dto);
    }
}
