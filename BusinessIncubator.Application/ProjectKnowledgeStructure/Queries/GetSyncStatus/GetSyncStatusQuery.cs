using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetSyncStatus;

/// <summary>
/// Query to get synchronization status for a project.
/// </summary>
public class GetSyncStatusQuery : IBaseRequest<SyncStatusDto>
{
    /// <summary>
    /// Gets or sets the project ID to get sync status for.
    /// </summary>
    public long ProjectId { get; set; }
}