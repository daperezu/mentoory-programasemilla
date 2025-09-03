using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when a diagnostics module is updated.
/// </summary>
public record DiagnosticsModuleUpdatedIntegrationEvent : IntegrationEvent, INotification
{
    /// <summary>
    /// Gets or sets the module ID.
    /// </summary>
    public long ModuleId { get; set; }

    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the module description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the module order.
    /// </summary>
    public int Order { get; set; }
}