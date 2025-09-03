using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SubmitFormByExternalId;

/// <summary>
/// Command to submit a form by external ID.
/// </summary>
public sealed record SubmitFormByExternalIdCommand(Guid ExternalId, string IpAddress) : IBaseRequest<SubmitFormResult>;

/// <summary>
/// Result DTO for submit form operation.
/// </summary>
public sealed class SubmitFormResult
{
    public bool Success { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Validator for SubmitFormByExternalIdCommand.
/// </summary>
public class SubmitFormByExternalIdCommandValidator : AbstractValidator<SubmitFormByExternalIdCommand>
{
    public SubmitFormByExternalIdCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("El ID externo del formulario es requerido.");
    }
}

/// <summary>
/// Handler for SubmitFormByExternalIdCommand.
/// </summary>
public class SubmitFormByExternalIdCommandHandler(IBusinessIncubatorRepository repository, ITimeProvider timeProvider)
    : BaseCommandHandler<SubmitFormByExternalIdCommand, SubmitFormResult>
{
    public override async Task<Result<SubmitFormResult>> Handle(SubmitFormByExternalIdCommand request, CancellationToken cancellationToken)
    {
        // Get submission by external ID
        var submission = await repository.GetFormSubmissionByExternalIdAsync(request.ExternalId, cancellationToken);

        if (submission is null)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NotFound, (nameof(request.ExternalId), "El formulario no fue encontrado."));
        }

        // Check if submission can be submitted
        if (submission.Status != Domain.Enums.ProjectFormSubmissionStatus.Draft)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_AlreadySubmitted, ("Status", "El formulario ya ha sido enviado o no está en estado de borrador."));
        }

        // Check if draft data exists
        if (string.IsNullOrWhiteSpace(submission.DraftData))
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NoDraftData, ("DraftData", "El formulario no tiene datos para enviar."));
        }

        // Check completion percentage
        if (submission.CompletionPercentage < 100)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_Incomplete, ("CompletionPercentage", $"El formulario está {submission.CompletionPercentage}% completo. Debe completar todas las preguntas requeridas."));
        }

        // Submit the form
        var submittedAt = timeProvider.UtcNow;
        submission.Submit(submittedAt);

        // Save changes
        await repository.UpdateFormSubmissionAsync(submission, cancellationToken);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return Success(new SubmitFormResult
        {
            Success = true,
            SubmittedAt = submittedAt,
            Status = submission.Status.ToString()
        });
    }
}
