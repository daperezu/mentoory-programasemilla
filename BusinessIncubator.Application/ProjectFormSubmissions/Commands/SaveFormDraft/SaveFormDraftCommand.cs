using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveFormDraft;

/// <summary>
/// Command to save a form draft with progress tracking.
/// </summary>
public sealed record SaveFormDraftCommand(
    long FormSubmissionId,
    string DraftData,
    int AnsweredQuestions,
    int TotalQuestions) : IRequest<Result<bool>>;

/// <summary>
/// Validator for SaveFormDraftCommand.
/// </summary>
public class SaveFormDraftCommandValidator : AbstractValidator<SaveFormDraftCommand>
{
    public SaveFormDraftCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FormSubmissionId)
            .GreaterThan(0)
            .WithMessage("El ID del formulario debe ser válido.");

        RuleFor(x => x.DraftData)
            .NotEmpty()
            .WithMessage("Los datos del borrador son requeridos.");

        RuleFor(x => x.AnsweredQuestions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El número de preguntas respondidas no puede ser negativo.");

        RuleFor(x => x.TotalQuestions)
            .GreaterThan(0)
            .WithMessage("El número total de preguntas debe ser mayor que cero.");

        RuleFor(x => x)
            .Must(x => x.AnsweredQuestions <= x.TotalQuestions)
            .WithMessage("El número de preguntas respondidas no puede ser mayor que el total.");
    }
}

/// <summary>
/// Handler for SaveFormDraftCommand.
/// </summary>
public class SaveFormDraftCommandHandler(
    IBusinessIncubatorRepository businessIncubatorRepository,
    ITimeProvider timeProvider)
    : BaseCommandHandler<SaveFormDraftCommand, bool>
{
    public override async Task<Result<bool>> Handle(
        SaveFormDraftCommand request,
        CancellationToken cancellationToken)
    {
        // Get form submission
        var submission = await businessIncubatorRepository.GetFormSubmissionByIdAsync(
            request.FormSubmissionId,
            cancellationToken);

        if (submission is null)
        {
            return Failure(
                ResultErrorCodes.ProjectFormSubmission_NotFound,
                (nameof(request.FormSubmissionId), "El formulario no fue encontrado."));
        }

        // Validate status is Draft
        if (submission.Status != ProjectFormSubmissionStatus.Draft)
        {
            return Failure(
                ResultErrorCodes.ProjectFormSubmission_InvalidStatus,
                ("Status", "Solo se pueden guardar borradores cuando el formulario está en estado borrador."));
        }

        try
        {
            // Update draft data and progress
            var autoSaveTime = timeProvider.UtcNow;
            submission.SaveDraft(
                request.DraftData,
                request.AnsweredQuestions,
                request.TotalQuestions,
                autoSaveTime);

            // Save changes
            await businessIncubatorRepository.UpdateFormSubmissionAsync(submission, cancellationToken);
            await businessIncubatorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success(true);
        }
        catch (ArgumentException ex)
        {
            return Failure(
                ResultErrorCodes.ProjectFormSubmission_SaveFailed,
                ("Validation", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Failure(
                ResultErrorCodes.ProjectFormSubmission_SaveFailed,
                ("Operation", ex.Message));
        }
    }
}