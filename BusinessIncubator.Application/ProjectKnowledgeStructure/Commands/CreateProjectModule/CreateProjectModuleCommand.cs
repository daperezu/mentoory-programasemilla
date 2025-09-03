using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectModule;

/// <summary>
/// Command to create a new project module.
/// </summary>
public sealed record CreateProjectModuleCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    string Name,
    int Order) : IBaseRequest<long>;