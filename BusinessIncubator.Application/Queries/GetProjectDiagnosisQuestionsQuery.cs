using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Queries;

public sealed record GetProjectDiagnosisQuestionsQuery(
    Guid ProjectExternalId,
    int DiagnosisPhase) : IBaseRequest<List<DiagnosisBlockDto>>;

public sealed record DiagnosisBlockDto(
    long BlockId,
    string Title,
    List<DiagnosisQuestionDto> Questions);

public sealed record DiagnosisQuestionDto(
    long QuestionId,
    string Text,
    int AnswerType,
    bool IsRequired,
    List<DiagnosisAnswerOptionDto> AnswerOptions);

public sealed record DiagnosisAnswerOptionDto(
    long AnswerOptionId,
    string Text,
    string? FollowUpQuestionText);

public sealed class GetProjectDiagnosisQuestionsQueryHandler(
    IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectDiagnosisQuestionsQuery, List<DiagnosisBlockDto>>
{
    public override async Task<Result<List<DiagnosisBlockDto>>> Handle(
        GetProjectDiagnosisQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var diagnosisBlocks = await repository.GetProjectDiagnosisQuestionsAsync(
            request.ProjectExternalId,
            request.DiagnosisPhase,
            cancellationToken);

        if (!diagnosisBlocks.Any())
        {
            // Check if project exists
            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, ("ProjectExternalId", "Proyecto no encontrado"));
            }

            return Success(new List<DiagnosisBlockDto>());
        }

        var result = diagnosisBlocks.Select(block => new DiagnosisBlockDto(
            block.Id,
            block.Title,
            block.Questions.Select(q => new DiagnosisQuestionDto(
                q.Id,
                q.Text,
                q.AnswerType,
                true, // IsRequired - default to true for diagnosis questions
                q.Options.Select(ao => new DiagnosisAnswerOptionDto(
                    ao.Id,
                    ao.Text,
                    ao.FollowUpQuestionText)).ToList())).ToList())).ToList();

        return Success(result);
    }
}