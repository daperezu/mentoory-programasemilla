using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectFormSubmissions.Queries.GetProjectFormStructure;

/// <summary>
/// Query to get the form structure for a project.
/// </summary>
public sealed record GetProjectFormStructureQuery : IBaseRequest<ProjectFormStructureDto>
{
    /// <summary>
    /// Gets the project ID.
    /// </summary>
    public long ProjectId { get; init; }

    /// <summary>
    /// Gets the form ID.
    /// </summary>
    public long FormId { get; init; }
}