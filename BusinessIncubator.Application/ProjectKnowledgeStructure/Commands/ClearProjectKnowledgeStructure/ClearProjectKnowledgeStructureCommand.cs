using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.ClearProjectKnowledgeStructure;

/// <summary>
/// Command to clear all knowledge structure data from a project.
/// </summary>
public sealed record ClearProjectKnowledgeStructureCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId) : IBaseRequest;