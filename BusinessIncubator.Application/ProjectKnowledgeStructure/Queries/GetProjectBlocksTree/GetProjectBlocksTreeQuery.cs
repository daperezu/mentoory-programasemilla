using LinaSys.Shared.Application.MediatR;
using LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructureTree;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectBlocksTree;

/// <summary>
/// Query to get the project blocks as a tree.
/// </summary>
public sealed record GetProjectBlocksTreeQuery(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId) : IBaseRequest<List<TreeNodeDto>>;