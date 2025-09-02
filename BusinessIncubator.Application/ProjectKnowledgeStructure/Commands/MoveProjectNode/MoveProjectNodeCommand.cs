using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.MoveProjectNode;

/// <summary>
/// Command to move a node in the project knowledge structure.
/// </summary>
/// <param name="BusinessIncubatorExternalId">The business incubator external ID.</param>
/// <param name="ProjectExternalId">The project external ID.</param>
/// <param name="NodeId">The node ID to move.</param>
/// <param name="NodeType">The type of node (module, topic, subject, block, question).</param>
/// <param name="ParentId">The new parent ID.</param>
/// <param name="ParentType">The parent type.</param>
/// <param name="Position">The new position within the parent.</param>
public sealed record MoveProjectNodeCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    string NodeId,
    string NodeType,
    string? ParentId,
    string? ParentType,
    int Position) : IBaseRequest;