using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectTopic;

/// <summary>
/// Command to create a project topic.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="ModuleId">The module ID where the topic will be created.</param>
/// <param name="Name">The topic name.</param>
/// <param name="Order">The display order.</param>
public sealed record CreateProjectTopicCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long ModuleId,
    string Name,
    int Order) : IBaseRequest<long>;