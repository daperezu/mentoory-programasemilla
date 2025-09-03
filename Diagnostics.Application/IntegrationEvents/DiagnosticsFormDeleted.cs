using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a diagnostics form is deleted.
/// This event is used to clear source references in projects.
/// </summary>
public sealed record DiagnosticsFormDeleted(
    long FormId) : IntegrationEvent, INotification;
