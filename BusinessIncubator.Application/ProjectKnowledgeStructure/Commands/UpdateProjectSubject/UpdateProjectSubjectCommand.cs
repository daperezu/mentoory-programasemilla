using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectSubject;

/// <summary>
/// Command to update a project subject.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="SubjectId">The subject ID to update.</param>
/// <param name="Name">The new name for the subject.</param>
/// <param name="Content">The new content for the subject.</param>
/// <param name="Order">The new order for the subject.</param>
public sealed record UpdateProjectSubjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long SubjectId,
    string Name,
    string? Content,
    int Order) : IBaseRequest;