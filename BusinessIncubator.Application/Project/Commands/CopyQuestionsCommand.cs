using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

public sealed record CopyQuestionsCommand(Guid BusinessIncubatorExternalId, Guid ProjectExternalId, List<QuestionToCopyDto> Questions) : IBaseRequest;

public sealed record QuestionToCopyDto(
    long? SourceTopicId,
    long SourceBlockId,
    long? SourceQuestionId,
    string Text,
    int AnswerType,
    int AppliesToPhase,
    bool IsUsedForMentoringPlan,
    bool IsUsedForDiagnosis,
    int Order,
    List<AnswerOptionToCopyDto> AnswerOptions);

public sealed record AnswerOptionToCopyDto(
    long? SourceAnswerOptionId,
    string Text,
    int Score,
    string Foda,
    string FodaExplanation,
    string Odsr,
    string OdsrExplanation,
    int Order,
    string? FollowUpQuestionText);

internal class CopyQuestionsCommandValidator : AbstractValidator<CopyQuestionsCommand>
{
    public CopyQuestionsCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");
        RuleFor(command => command.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");
        RuleFor(command => command.Questions)
            .NotEmpty()
            .WithMessage("Questions list must not be empty.");
    }
}

internal class CopyQuestionsCommandHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<CopyQuestionsCommand>
{
    public override async Task<Result> Handle(CopyQuestionsCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var businessIncubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found."));
        }

        // Get the project with knowledge structure
        var project = await repository.GetProjectWithKnowledgeStructureByExternalIdAsync(request.ProjectExternalId, cancellationToken).ConfigureAwait(false);

        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != businessIncubator.Id)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        var questionsToAdd = request.Questions
            .DistinctBy(question => new { question.SourceQuestionId, question.Text, question.SourceBlockId, question.SourceTopicId })
            .ToList();

        foreach (var questionDto in questionsToAdd)
        {
            // Find the appropriate block using domain methods
            var projectBlock = project.FindBlockBySourceId(questionDto.SourceBlockId);

            if (projectBlock is null)
            {
                return Failure(
                    ResultErrorCodes.Block_NotFound,
                    (nameof(questionDto.SourceBlockId), $"Block with source ID {questionDto.SourceBlockId} not found in project. The knowledge structure must be properly synchronized before copying questions."));
            }

            ProjectQuestion projectQuestion;

            if (questionDto.SourceTopicId.HasValue)
            {
                // Question belongs to a topic (and implicitly to its block)
                var projectTopic = project.FindTopicBySourceId(questionDto.SourceTopicId);

                if (projectTopic is null)
                {
                    return Failure(
                        ResultErrorCodes.Topic_NotFound,
                        (nameof(questionDto.SourceTopicId), $"Topic with source ID {questionDto.SourceTopicId} not found in project. The knowledge structure must be properly synchronized before copying questions."));
                }

                // Check if question already exists in the topic
                if (projectTopic.HasQuestionWithSourceId(questionDto.SourceQuestionId) ||
                    projectTopic.HasQuestionWithTextAndBlock(questionDto.Text, projectBlock.Id))
                {
                    continue; // Skip duplicates in topic
                }

                // Add question through topic (which will also set the block)
                projectQuestion = projectTopic.AddProjectQuestion(
                    projectBlock.Id,
                    questionDto.SourceQuestionId,
                    questionDto.Text,
                    false, // isTextCustomized
                    (Domain.Enums.AnswerType)questionDto.AnswerType,
                    false, // isAnswerTypeCustomized
                    (Domain.Enums.QuestionPhase)questionDto.AppliesToPhase,
                    false, // isAppliesToPhaseCustomized
                    questionDto.IsUsedForMentoringPlan,
                    false, // isMentoringPlanCustomized
                    questionDto.IsUsedForDiagnosis,
                    false, // isDiagnosisCustomized
                    questionDto.Order,
                    false); // isOrderCustomized
            }
            else
            {
                // Question belongs only to the block (no topic)
                // Check if question already exists in the block
                if (projectBlock.HasQuestionWithSourceId(questionDto.SourceQuestionId) ||
                    projectBlock.HasQuestionWithText(questionDto.Text))
                {
                    continue; // Skip duplicates in block
                }

                projectQuestion = projectBlock.AddProjectQuestion(
                    questionDto.SourceQuestionId,
                    questionDto.Text,
                    false, // isTextCustomized
                    (Domain.Enums.AnswerType)questionDto.AnswerType,
                    false, // isAnswerTypeCustomized
                    (Domain.Enums.QuestionPhase)questionDto.AppliesToPhase,
                    false, // isAppliesToPhaseCustomized
                    questionDto.IsUsedForMentoringPlan,
                    false, // isMentoringPlanCustomized
                    questionDto.IsUsedForDiagnosis,
                    false, // isDiagnosisCustomized
                    questionDto.Order,
                    false); // isOrderCustomized
            }

            // Add answer options
            foreach (var answerOptionDto in questionDto.AnswerOptions)
            {
                projectQuestion.AddProjectAnswerOption(
                    answerOptionDto.SourceAnswerOptionId,
                    answerOptionDto.Text,
                    false, // isTextCustomized
                    answerOptionDto.Score,
                    false, // isScoreCustomized
                    ParseFodaType(answerOptionDto.Foda),
                    false, // isFodaCustomized
                    answerOptionDto.FodaExplanation,
                    false, // isFodaExplanationCustomized
                    ParseOdsrType(answerOptionDto.Odsr),
                    false, // isOdsrCustomized
                    answerOptionDto.OdsrExplanation,
                    false, // isOdsrExplanationCustomized
                    answerOptionDto.Order,
                    false, // isOrderCustomized
                    answerOptionDto.FollowUpQuestionText ?? string.Empty,
                    false); // isFollowUpTextCustomized
            }
        }

        repository.Update(project);

        return Success();
    }

    private static Domain.Enums.FodaType ParseFodaType(string foda)
    {
        return foda switch
        {
            "F" => Domain.Enums.FodaType.Fortalezas,
            "O" => Domain.Enums.FodaType.Oportunidades,
            "D" => Domain.Enums.FodaType.Debilidades,
            "A" => Domain.Enums.FodaType.Amenazas,
            _ => Domain.Enums.FodaType.NoDefinido,
        };
    }

    private static Domain.Enums.OdsrType ParseOdsrType(string odsr)
    {
        return odsr switch
        {
            "O" => Domain.Enums.OdsrType.Ofensiva,
            "D" => Domain.Enums.OdsrType.Defensiva,
            "S" => Domain.Enums.OdsrType.Supervivencia,
            "R" => Domain.Enums.OdsrType.Reorientacion,
            _ => Domain.Enums.OdsrType.NoDefinido,
        };
    }
}
