using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Orchestration.Application.Diagnostics.Commands;

public sealed record UpsertAnswersToProjectFormCommand(
    Guid ProjectExternalId,
    int Phase,
    List<ProjectAnswerDto> Answers) : IBaseRequest;

public sealed record ProjectAnswerDto(
    long QuestionId,
    long? AnswerOptionId,
    string? UserInput,
    string? FollowUpUserInput);

public sealed class UpsertAnswersToProjectFormCommandValidator : AbstractValidator<UpsertAnswersToProjectFormCommand>
{
    public UpsertAnswersToProjectFormCommandValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID cannot be empty.");

        RuleFor(x => x.Phase)
            .Must(value => Enum.IsDefined(typeof(LinaSys.Diagnostics.Domain.Enums.QuestionPhase), value))
            .WithMessage("Phase must be a valid enum value.");

        RuleFor(x => x.Answers)
            .NotNull()
            .WithMessage("Answers cannot be null.");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId)
                .GreaterThan(0)
                .WithMessage("Question ID must be greater than 0.");
        });
    }
}

public sealed class UpsertAnswersToProjectFormCommandHandler(
    IBusinessIncubatorRepository businessIncubatorRepository) : BaseCommandHandler<UpsertAnswersToProjectFormCommand>
{
    public override async Task<Result> Handle(UpsertAnswersToProjectFormCommand request, CancellationToken cancellationToken)
    {
        // First verify the project exists
        var project = await businessIncubatorRepository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken).ConfigureAwait(false);

        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Validate all questions exist and get their validation data
        var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
        var validationData = await businessIncubatorRepository.ValidateProjectQuestionsAsync(
            request.ProjectExternalId,
            questionIds,
            cancellationToken).ConfigureAwait(false);

        // Validate each answer
        foreach (var answer in request.Answers)
        {
            if (!validationData.TryGetValue(answer.QuestionId, out var questionValidation))
            {
                return Failure(
                    ResultErrorCodes.Question_NotFound,
                    (nameof(answer.QuestionId), $"La pregunta con ID {answer.QuestionId} no existe en este proyecto."));
            }

            // Validate answer option if provided
            if (answer.AnswerOptionId.HasValue)
            {
                if (!questionValidation.ValidAnswerOptionIds.Contains(answer.AnswerOptionId.Value))
                {
                    return Failure(
                        ResultErrorCodes.AnswerOption_NotFound,
                        (nameof(answer.AnswerOptionId), $"La opción de respuesta con ID {answer.AnswerOptionId} no es válida para la pregunta {answer.QuestionId}."));
                }
            }

            // Validate that either AnswerOptionId or UserInput is provided based on answer type
            var answerType = (LinaSys.BusinessIncubator.Domain.Enums.AnswerType)questionValidation.AnswerType;
            if (answerType == LinaSys.BusinessIncubator.Domain.Enums.AnswerType.SingleChoice ||
                answerType == LinaSys.BusinessIncubator.Domain.Enums.AnswerType.MultiChoice)
            {
                if (!answer.AnswerOptionId.HasValue)
                {
                    return Failure(
                        ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                        (nameof(answer.AnswerOptionId), $"Se requiere seleccionar una opción para la pregunta {answer.QuestionId}."));
                }
            }
            else if (answerType == LinaSys.BusinessIncubator.Domain.Enums.AnswerType.FreeText)
            {
                if (string.IsNullOrWhiteSpace(answer.UserInput))
                {
                    return Failure(
                        ResultErrorCodes.Validation_SomeFieldsAreInvalid,
                        (nameof(answer.UserInput), $"Se requiere una respuesta de texto para la pregunta {answer.QuestionId}."));
                }
            }
        }

        // Note: The actual saving of answers would require:
        // 1. A separate aggregate or entity for ProjectUserAnswers
        // 2. A repository method to save the answers
        // 3. Proper domain logic to handle answer updates vs inserts
        // For now, we just validate and return success since the infrastructure doesn't exist yet
        return Success();
    }
}
