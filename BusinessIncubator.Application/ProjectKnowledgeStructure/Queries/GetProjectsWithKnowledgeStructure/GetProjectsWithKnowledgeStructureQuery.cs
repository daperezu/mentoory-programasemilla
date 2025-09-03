using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectsWithKnowledgeStructure;

/// <summary>
/// Query to get projects with knowledge structures from a business incubator.
/// </summary>
public sealed record GetProjectsWithKnowledgeStructureQuery(
    Guid BusinessIncubatorExternalId,
    Guid ExcludeProjectExternalId) : IBaseRequest<List<ProjectWithKnowledgeStructureDto>>;