using LinaSys.Shared.Application.MediatR;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectModule;

/// <summary>
/// Command to update a project module.
/// </summary>
public sealed record UpdateProjectModuleCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long ModuleId,
    string Name,
    int Order) : IBaseRequest;
