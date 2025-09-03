namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// Model for moving a node in the knowledge structure tree.
/// </summary>
public class MoveNodeModel
{
    /// <summary>
    /// Gets or sets the type of node being moved (block, question, module, topic, subject).
    /// </summary>
    public string NodeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the node being moved.
    /// </summary>
    public long NodeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the old parent node.
    /// </summary>
    public long? OldParentId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the new parent node.
    /// </summary>
    public long? NewParentId { get; set; }

    /// <summary>
    /// Gets or sets the old position of the node among its siblings.
    /// </summary>
    public int OldPosition { get; set; }

    /// <summary>
    /// Gets or sets the new position of the node among its siblings.
    /// </summary>
    public int NewPosition { get; set; }
}
