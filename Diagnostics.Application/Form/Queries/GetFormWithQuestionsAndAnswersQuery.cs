using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public record GetFormWithQuestionsAndAnswersQuery(long FormId) : IBaseRequest<FormDto>;

public class FormDto
{
    public long Id { get; set; }

    public string Name { get; set; }

    public long? SourceKnowledgeStructureId { get; set; }

    public List<QuestionsDto> Questions { get; set; }
}

public class QuestionsDto
{
    public long Id { get; set; }

    public long? TopicId { get; set; }

    public long BlockId { get; set; }

    public string BlockName { get; set; }

    public int Order { get; set; }

    public string Text { get; set; }

    public int AnswerType { get; set; }

    public int AppliesToPhase { get; set; }

    public bool IsUsedForMentoringPlan { get; set; }

    public bool IsUsedForDiagnosis { get; set; }

    public List<AnswerDto> Answers { get; set; } = [];
}

public class AnswerDto
{
    public long Id { get; set; }

    public string Text { get; set; }

    public int Score { get; set; }

    public string Foda { get; set; }

    public string FodaExplanation { get; set; }

    public string Odsr { get; set; }

    public string OdsrExplanation { get; set; }

    public int Order { get; set; }

    public string? FollowUpQuestionText { get; set; }
}

public class GetFormWithQuestionsAndAnswersQueryValidator : AbstractValidator<GetFormWithQuestionsAndAnswersQuery>
{
    public GetFormWithQuestionsAndAnswersQueryValidator()
    {
        RuleFor(query => query.FormId)
            .GreaterThan(0)
            .WithMessage("Form ID must be greater than 0.");
    }
}

public class GetFormWithQuestionsAndAnswersQueryHandler(IFormRepository formRepository) : BaseCommandHandler<GetFormWithQuestionsAndAnswersQuery, FormDto>
{
    public override async Task<Result<FormDto>> Handle(GetFormWithQuestionsAndAnswersQuery request, CancellationToken cancellationToken)
    {
        var form = await formRepository.GetByIdWithBlocksQuestionsAndAnswersAsync(request.FormId, cancellationToken).ConfigureAwait(false);

        if (form is null)
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NotFound, ("Form", "Form not found."));
        }

        var result = new FormDto
        {
            Id = form.Id,
            Name = form.Name,
            SourceKnowledgeStructureId = form.SourceKnowledgeStructureId,
            Questions = form.FormQuestions.Select(formQuestion => new QuestionsDto
                {
                    Id = formQuestion.QuestionId,
                    TopicId = formQuestion.TopicId,
                    BlockId = formQuestion.BlockId,
                    BlockName = formQuestion.Block.Name,
                    Order = formQuestion.Order,
                    Text = formQuestion.Question.Text,
                    AnswerType = (int)formQuestion.Question.AnswerType,
                    AppliesToPhase = (int)formQuestion.Question.AppliesToPhase,
                    IsUsedForMentoringPlan = formQuestion.Question.IsUsedForMentoringPlan,
                    IsUsedForDiagnosis = formQuestion.Question.IsUsedForDiagnosis,
                    Answers = formQuestion.Question.AnswerOptions.Select(answer => new AnswerDto
                        {
                            Id = answer.Id,
                            Text = answer.Text,
                            Score = answer.Score,
                            Foda = answer.Foda.ToString()[1].ToString(),
                            FodaExplanation = answer.FodaExplanation,
                            Odsr = answer.Odsr.ToString()[1].ToString(),
                            OdsrExplanation = answer.OdsrExplanation,
                            Order = answer.Order,
                            FollowUpQuestionText = answer.FollowUpQuestionText,
                        })
                        .ToList(),
                })
                .ToList(),
        };

        return Success(result);
    }
}
