using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectQuestion;

/// <summary>
/// Command to create a new project question.
/// </summary>
public sealed record CreateProjectQuestionCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long BlockId,
    string Text,
    AnswerType AnswerType,
    bool IsUsedForDiagnosis,
    QuestionPhase AppliesToPhase,
    int Order,
    long? TopicId = null) : IBaseRequest<long>;