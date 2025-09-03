using System.Text.Json;
using FluentValidation;
using LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.DTOs;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveFormDraftByExternalId;

/// <summary>
/// Command to save a form draft by external ID.
/// </summary>
public sealed record SaveFormDraftByExternalIdCommand(
    Guid ExternalId,
    DraftDataDto DraftData,
    int AnsweredQuestions,
    int TotalQuestions) : IBaseRequest<SaveFormDraftResult>;

/// <summary>
/// Result DTO for save draft operation.
/// </summary>
public sealed class SaveFormDraftResult
{
    public bool Success { get; set; }
    public DateTime LastSavedAt { get; set; }
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Validator for SaveFormDraftByExternalIdCommand.
/// </summary>
public class SaveFormDraftByExternalIdCommandValidator : AbstractValidator<SaveFormDraftByExternalIdCommand>
{
    public SaveFormDraftByExternalIdCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("El ID externo del formulario es requerido.");

        RuleFor(x => x.DraftData)
            .NotNull()
            .WithMessage("Los datos del borrador son requeridos.");

        RuleFor(x => x.AnsweredQuestions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El número de preguntas respondidas debe ser mayor o igual a 0.");

        RuleFor(x => x.TotalQuestions)
            .GreaterThan(0)
            .WithMessage("El número total de preguntas debe ser mayor a 0.");
    }
}

/// <summary>
/// Handler for SaveFormDraftByExternalIdCommand.
/// </summary>
public class SaveFormDraftByExternalIdCommandHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider)
    : BaseCommandHandler<SaveFormDraftByExternalIdCommand, SaveFormDraftResult>
{
    public override async Task<Result<SaveFormDraftResult>> Handle(
        SaveFormDraftByExternalIdCommand request,
        CancellationToken cancellationToken)
    {
        // Get submission by external ID
        var submission = await repository.GetFormSubmissionByExternalIdAsync(request.ExternalId, cancellationToken);

        if (submission is null)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_NotFound, (nameof(request.ExternalId), "El formulario no fue encontrado."));
        }

        // Check if submission can be edited
        if (submission.Status != Domain.Enums.ProjectFormSubmissionStatus.Draft &&
            submission.Status != Domain.Enums.ProjectFormSubmissionStatus.Rejected)
        {
            return Failure(ResultErrorCodes.ProjectFormSubmission_CannotEdit, ("Status", "El formulario no puede ser editado en su estado actual."));
        }

        // Serialize draft data
        var draftDataJson = JsonSerializer.Serialize(request.DraftData);

        // Update submission
        submission.SaveDraft(
            draftDataJson,
            request.AnsweredQuestions,
            request.TotalQuestions,
            timeProvider.UtcNow);

        // Save changes
        await repository.UpdateFormSubmissionAsync(submission, cancellationToken);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return Success(new SaveFormDraftResult
        {
            Success = true,
            LastSavedAt = submission.LastAutoSaveAt ?? timeProvider.UtcNow,
            CompletionPercentage = submission.CompletionPercentage
        });
    }
}
