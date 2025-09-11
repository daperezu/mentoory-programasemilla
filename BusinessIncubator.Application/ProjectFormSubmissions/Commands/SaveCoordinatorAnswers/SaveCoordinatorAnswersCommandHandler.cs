using System.Text.Json;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Commands.SaveCoordinatorAnswers;

/// <summary>
/// Handler for saving coordinator's answers during form review.
/// </summary>
public sealed class SaveCoordinatorAnswersCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SaveCoordinatorAnswersCommandHandler> logger,
    ITimeProvider timeProvider) : BaseCommandHandler<SaveCoordinatorAnswersCommand>
{
    public override async Task<Result> Handle(
        SaveCoordinatorAnswersCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the submission
            var submission = await repository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);
            if (submission is null)
            {
                return Failure(ResultErrorCodes.DiagnosisForm_NotFound,
                    (nameof(request.SubmissionId), "El formulario no existe."));
            }

            // Verify submission is in correct status
            if (submission.Status != ProjectFormSubmissionStatus.Submitted)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    ("Status", "Solo se pueden revisar formularios que han sido enviados."));
            }

            // Validate coordinator has answered all required questions
            var validation = ValidateCoordinatorAnswers(request.CoordinatorData);
            if (!validation.IsValid)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                    validation.Errors.Select(e => ("Validation", e)).ToArray());
            }

            // Serialize coordinator data
            var coordinatorDataJson = JsonSerializer.Serialize(request.CoordinatorData);

            // Save coordinator review
            submission.SaveCoordinatorReview(
                request.CoordinatorUserId,
                coordinatorDataJson,
                timeProvider.UtcNow);

            // No need to call Update for entities already tracked
            // The submission is already tracked after retrieval
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Coordinator {CoordinatorUserId} saved answers for submission {SubmissionId}",
                request.CoordinatorUserId,
                request.SubmissionId);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation saving coordinator answers for submission {SubmissionId}",
                request.SubmissionId);
            return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("BusinessRule", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving coordinator answers for submission {SubmissionId}",
                request.SubmissionId);
            return Failure(ResultErrorCodes.Unknown,
                ("SaveCoordinatorAnswers", "Error al guardar las respuestas del coordinador."));
        }
    }

    private ValidationResult ValidateCoordinatorAnswers(ProjectFormSubmissions.Commands.SaveDraft.DraftDataDto coordinatorData)
    {
        var errors = new List<string>();

        // Verify there are responses
        if (coordinatorData.BlockResponses == null || !coordinatorData.BlockResponses.Any())
        {
            errors.Add("No se encontraron respuestas del coordinador.");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Count total questions and answered questions
        var totalQuestions = 0;
        var answeredQuestions = 0;

        foreach (var block in coordinatorData.BlockResponses)
        {
            if (block.QuestionResponses != null)
            {
                totalQuestions += block.QuestionResponses.Count;
                answeredQuestions += block.QuestionResponses.Count(q => q.IsAnswered);
            }
        }

        // Verify all questions are answered
        if (answeredQuestions < totalQuestions)
        {
            errors.Add($"El coordinador debe responder todas las preguntas. Respondidas: {answeredQuestions}/{totalQuestions}");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}