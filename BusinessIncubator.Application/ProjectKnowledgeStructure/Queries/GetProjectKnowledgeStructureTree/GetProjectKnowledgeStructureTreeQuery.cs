using LinaSys.Shared.Application.MediatR;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;

/// <summary>
/// Query to get the project knowledge structure as a tree.
/// </summary>
public sealed record GetProjectKnowledgeStructureTreeQuery(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId) : IBaseRequest<List<TreeNodeDto>>;
