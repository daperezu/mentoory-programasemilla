using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectQuestion;

/// <summary>
/// Command to delete a project question.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="QuestionId">The question ID to delete.</param>
public sealed record DeleteProjectQuestionCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long QuestionId) : IBaseRequest;