using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectQuestion;

/// <summary>
/// Command to update a project question.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="QuestionId">The question ID to update.</param>
/// <param name="Text">The new text for the question.</param>
/// <param name="AnswerType">The answer type.</param>
/// <param name="IsUsedForDiagnosis">Whether the question is used for diagnosis.</param>
/// <param name="AppliesToPhase">The phase the question applies to.</param>
/// <param name="Order">The order of the question.</param>
/// <param name="TopicId">Optional topic ID to link the question to.</param>
public sealed record UpdateProjectQuestionCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long QuestionId,
    string Text,
    AnswerType AnswerType,
    bool IsUsedForDiagnosis,
    QuestionPhase AppliesToPhase,
    int Order,
    long? TopicId = null) : IBaseRequest;