using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectTopic;

/// <summary>
/// Command to update a project topic.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="TopicId">The topic ID to update.</param>
/// <param name="Name">The new name for the topic.</param>
/// <param name="Order">The new order for the topic.</param>
public sealed record UpdateProjectTopicCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long TopicId,
    string Name,
    int Order) : IBaseRequest;