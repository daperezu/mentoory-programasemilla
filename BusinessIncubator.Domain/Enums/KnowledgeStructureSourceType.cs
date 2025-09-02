namespace LinaSys.BusinessIncubator.Domain.Enums;

/// <summary>
/// Represents the source type for copying knowledge structures.
/// </summary>
public enum KnowledgeStructureSourceType
{
    /// <summary>
    /// Copy from global diagnostic forms.
    /// </summary>
    Global = 1,

    /// <summary>
    /// Copy from another project within the same incubator.
    /// </summary>
    Project = 2
}