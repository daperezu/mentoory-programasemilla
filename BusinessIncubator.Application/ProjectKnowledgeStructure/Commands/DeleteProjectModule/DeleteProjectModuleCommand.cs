using LinaSys.Shared.Application.MediatR;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectModule;

/// <summary>
/// Command to delete a project module.
/// </summary>
public sealed record DeleteProjectModuleCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long ModuleId) : IBaseRequest;
