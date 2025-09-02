using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectAnswerOption;

/// <summary>
/// Command to delete an answer option from a project question.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="AnswerOptionId">The answer option ID to delete.</param>
public sealed record DeleteProjectAnswerOptionCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long AnswerOptionId) : IBaseRequest;