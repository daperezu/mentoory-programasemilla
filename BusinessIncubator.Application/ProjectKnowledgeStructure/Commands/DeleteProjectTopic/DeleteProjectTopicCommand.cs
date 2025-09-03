using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectTopic;

/// <summary>
/// Command to delete a project topic.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="TopicId">The topic ID to delete.</param>
public sealed record DeleteProjectTopicCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long TopicId) : IBaseRequest;