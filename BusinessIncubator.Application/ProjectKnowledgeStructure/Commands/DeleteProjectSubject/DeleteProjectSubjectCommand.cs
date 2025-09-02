using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectSubject;

/// <summary>
/// Command to delete a project subject.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="SubjectId">The subject ID to delete.</param>
public sealed record DeleteProjectSubjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long SubjectId) : IBaseRequest;