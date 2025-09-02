using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when a diagnostics module is deleted.
/// </summary>
public record DiagnosticsModuleDeletedIntegrationEvent : IntegrationEvent, INotification
{
    /// <summary>
    /// Gets or sets the module ID that was deleted.
    /// </summary>
    public long ModuleId { get; set; }
}