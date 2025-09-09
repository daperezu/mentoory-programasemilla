using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectFormStructure;

/// <summary>
/// Query to get the project form structure from the project's knowledge structure.
/// </summary>
public sealed record GetProjectFormStructureQuery : IBaseRequest<ProjectFormStructureDto>
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }
}