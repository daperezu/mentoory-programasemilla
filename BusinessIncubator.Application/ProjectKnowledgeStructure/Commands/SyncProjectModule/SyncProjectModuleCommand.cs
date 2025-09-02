using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectModule;

/// <summary>
/// Command to synchronize a project module with its source.
/// </summary>
public sealed record SyncProjectModuleCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long ModuleId) : IBaseRequest;