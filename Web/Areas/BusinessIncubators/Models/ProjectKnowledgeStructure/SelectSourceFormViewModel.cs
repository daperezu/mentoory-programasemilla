using LinaSys.BusinessIncubator.Domain.Enums;

namespace LinaSys.Web.Areas.BusinessIncubators.Models.ProjectKnowledgeStructure;

/// <summary>
/// View model for selecting a source form to copy.
/// </summary>
public class SelectSourceFormViewModel
{
    /// <summary>
    /// Gets or sets the business incubator ID.
    /// </summary>
    public Guid BusinessIncubatorId { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the available source types.
    /// </summary>
    public List<SourceTypeOption> SourceTypes { get; set; } =
    [
        new SourceTypeOption { Value = ((int)KnowledgeStructureSourceType.Global).ToString(), Text = "Formularios Globales", Description = "Formularios de diagnóstico disponibles para todas las incubadoras" },
        new SourceTypeOption { Value = ((int)KnowledgeStructureSourceType.Project).ToString(), Text = "Otros Proyectos", Description = "Copiar estructura de otro proyecto de esta incubadora" }
    ];

    /// <summary>
    /// Gets or sets the available forms.
    /// </summary>
    public List<SourceFormOption> AvailableForms { get; set; } = [];
}

/// <summary>
/// Represents a source type option.
/// </summary>
public class SourceTypeOption
{
    /// <summary>
    /// Gets or sets the option value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the option text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the option description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Represents a source form option.
/// </summary>
public class SourceFormOption
{
    /// <summary>
    /// Gets or sets the form ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the form name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form type (global or incubator).
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
