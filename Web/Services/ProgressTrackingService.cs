using System.Collections.Concurrent;
using LinaSys.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace LinaSys.Web.Services;

/// <summary>
/// Implementation of the progress tracking service using memory cache and SignalR.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProgressTrackingService"/> class.
/// </remarks>
/// <param name="cache">The memory cache.</param>
/// <param name="hubContext">The SignalR hub context.</param>
/// <param name="logger">The logger.</param>
public class ProgressTrackingService(
    IMemoryCache cache,
    IHubContext<UserManagementHub> hubContext,
    ILogger<ProgressTrackingService> logger) : IProgressTrackingService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public IProgressTracker StartOperation(string operationId, int totalItems, string userId, string? description = null)
    {
        var cts = new CancellationTokenSource();
        _cancellationTokens[operationId] = cts;

        var progress = new BulkOperationProgress
        {
            OperationId = operationId,
            UserId = userId,
            Description = description,
            TotalItems = totalItems,
            ProcessedItems = 0,
            SuccessCount = 0,
            FailureCount = 0,
            CurrentMessage = "Iniciando operación...",
            StartTime = DateTime.UtcNow,
            IsCompleted = false,
            IsCancelled = false,
        };

        cache.Set($"bulk_operation_{operationId}", progress, _cacheExpiration);
        logger.LogInformation("Started operation {OperationId} with {TotalItems} items for user {UserId}",
            operationId, totalItems, userId);

        // Send initial progress notification
        _ = SendProgressUpdateAsync(progress);

        return new ProgressTracker(this, operationId, cts.Token);
    }

    /// <inheritdoc />
    public BulkOperationProgress? GetProgress(string operationId)
    {
        return cache.Get<BulkOperationProgress>($"bulk_operation_{operationId}");
    }

    /// <inheritdoc />
    public bool CancelOperation(string operationId)
    {
        if (_cancellationTokens.TryRemove(operationId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();

            var progress = GetProgress(operationId);
            if (progress != null && !progress.IsCompleted)
            {
                progress.IsCancelled = true;
                progress.IsCompleted = true;
                progress.EndTime = DateTime.UtcNow;
                progress.CurrentMessage = "Operación cancelada";
                cache.Set($"bulk_operation_{operationId}", progress, _cacheExpiration);
                _ = SendProgressUpdateAsync(progress);
            }

            logger.LogInformation("Cancelled operation {OperationId}", operationId);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public int CleanupOldOperations(TimeSpan olderThan)
    {
        // Note: IMemoryCache doesn't provide enumeration capabilities
        // In a production scenario, you might want to track operation IDs separately
        // or use a different caching strategy
        logger.LogInformation("Cleanup requested for operations older than {OlderThan}", olderThan);
        return 0;
    }

    internal async Task UpdateProgressAsync(string operationId, Action<BulkOperationProgress> updateAction)
    {
        var progress = GetProgress(operationId);
        if (progress == null)
        {
            logger.LogWarning("Progress not found for operation {OperationId}", operationId);
            return;
        }

        updateAction(progress);
        cache.Set($"bulk_operation_{operationId}", progress, _cacheExpiration);
        await SendProgressUpdateAsync(progress);
    }

    private async Task SendProgressUpdateAsync(BulkOperationProgress progress)
    {
        try
        {
            // Send to specific user
            await hubContext.Clients.User(progress.UserId).SendAsync("BulkOperationProgress", new
            {
                batchId = progress.OperationId,
                progress = progress.ProgressPercentage,
                message = progress.CurrentMessage,
                currentItem = progress.ProcessedItems,
                totalItems = progress.TotalItems,
                successCount = progress.SuccessCount,
                failureCount = progress.FailureCount,
                timestamp = DateTime.UtcNow,
                isCompleted = progress.IsCompleted,
                isCancelled = progress.IsCancelled,
                estimatedTimeRemaining = progress.EstimatedTimeRemaining?.TotalSeconds
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending progress update for operation {OperationId}", progress.OperationId);
        }
    }

    /// <summary>
    /// Internal implementation of progress tracker.
    /// </summary>
    private class ProgressTracker(
        ProgressTrackingService service,
        string operationId,
        CancellationToken cancellationToken) : IProgressTracker
    {
        private bool _disposed;

        public string OperationId => operationId;
        public CancellationToken CancellationToken => cancellationToken;

        public async Task ReportSuccessAsync(string itemDescription)
        {
            if (_disposed)
            {
                return;
            }

            await service.UpdateProgressAsync(operationId, progress =>
            {
                progress.ProcessedItems++;
                progress.SuccessCount++;
                progress.CurrentMessage = $"Procesado: {itemDescription}";
            });
        }

        public async Task ReportFailureAsync(string itemDescription, string error)
        {
            if (_disposed)
            {
                return;
            }

            await service.UpdateProgressAsync(operationId, progress =>
            {
                progress.ProcessedItems++;
                progress.FailureCount++;
                progress.CurrentMessage = $"Error en: {itemDescription}";
                progress.Errors.Add($"{itemDescription}: {error}");
            });
        }

        public async Task ReportProgressAsync(string message)
        {
            if (_disposed)
            {
                return;
            }

            await service.UpdateProgressAsync(operationId, progress =>
            {
                progress.CurrentMessage = message;
            });
        }

        public async Task CompleteAsync(string? summary = null)
        {
            if (_disposed)
            {
                return;
            }

            await service.UpdateProgressAsync(operationId, progress =>
            {
                progress.IsCompleted = true;
                progress.EndTime = DateTime.UtcNow;
                progress.CurrentMessage = summary ??
                    $"Operación completada: {progress.SuccessCount} exitosos, {progress.FailureCount} errores";
            });

            // Clean up cancellation token
            if (service._cancellationTokens.TryRemove(operationId, out var cts))
            {
                cts.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Auto-complete if not already completed
            if (!service.GetProgress(operationId)?.IsCompleted ?? false)
            {
                _ = CompleteAsync("Operación finalizada");
            }
        }
    }
}
