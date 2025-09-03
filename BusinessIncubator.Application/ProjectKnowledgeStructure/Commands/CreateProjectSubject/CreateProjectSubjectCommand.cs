using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectSubject;

/// <summary>
/// Command to create a project subject.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="TopicId">The topic ID where the subject will be created.</param>
/// <param name="Name">The subject name.</param>
/// <param name="Content">The subject content.</param>
/// <param name="Order">The display order.</param>
public sealed record CreateProjectSubjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long TopicId,
    string Name,
    string? Content,
    int Order) : IBaseRequest<long>;