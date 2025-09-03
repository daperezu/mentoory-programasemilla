using LinaSys.Shared.Application.MediatR;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructure;

/// <summary>
/// Query to get the project knowledge structure.
/// </summary>
public sealed record GetProjectKnowledgeStructureQuery(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId) : IBaseRequest<ProjectKnowledgeStructureDto>;
