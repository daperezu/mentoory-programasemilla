using LinaSys.Shared.Application.IntegrationEvents;
using MediatR;

namespace LinaSys.Diagnostics.Application.IntegrationEvents;

/// <summary>
/// Integration event that is published when a diagnostics form is updated.
/// This event is used to synchronize projects that use this form as source.
/// </summary>
public sealed record DiagnosticsFormUpdated(
    long FormId,
    string Name) : IntegrationEvent, INotification;
