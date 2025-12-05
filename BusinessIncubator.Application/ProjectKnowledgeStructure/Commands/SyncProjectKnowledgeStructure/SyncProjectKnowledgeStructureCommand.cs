using LinaSys.BusinessIncubator.Domain.ValueObjects;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectKnowledgeStructure;

/// <summary>
/// Command to synchronize a project's knowledge structure with its sources.
/// </summary>
public class SyncProjectKnowledgeStructureCommand : IBaseRequest<SyncResult>
{
    /// <summary>
    /// Gets or sets the project ID to sync.
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether this is a preview only (don't apply changes).
    /// </summary>
    public bool Preview { get; set; }
}