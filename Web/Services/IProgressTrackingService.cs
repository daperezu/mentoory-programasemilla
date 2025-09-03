namespace LinaSys.Web.Services;

/// <summary>
/// Service for tracking the progress of bulk operations.
/// </summary>
public interface IProgressTrackingService
{
    /// <summary>
    /// Starts tracking a new bulk operation.
    /// </summary>
    /// <param name="operationId">The unique identifier for the operation.</param>
    /// <param name="totalItems">The total number of items to process.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    /// <param name="description">Optional description of the operation.</param>
    /// <returns>A progress tracker instance.</returns>
    IProgressTracker StartOperation(string operationId, int totalItems, string userId, string? description = null);

    /// <summary>
    /// Gets the current progress of an operation.
    /// </summary>
    /// <param name="operationId">The operation identifier.</param>
    /// <returns>The current progress information, or null if not found.</returns>
    BulkOperationProgress? GetProgress(string operationId);

    /// <summary>
    /// Cancels an ongoing operation.
    /// </summary>
    /// <param name="operationId">The operation identifier.</param>
    /// <returns>True if the operation was cancelled, false if not found or already completed.</returns>
    bool CancelOperation(string operationId);

    /// <summary>
    /// Cleans up completed operations older than the specified timespan.
    /// </summary>
    /// <param name="olderThan">The age threshold for cleanup.</param>
    /// <returns>The number of operations cleaned up.</returns>
    int CleanupOldOperations(TimeSpan olderThan);
}

/// <summary>
/// Interface for tracking progress of individual operations.
/// </summary>
public interface IProgressTracker : IDisposable
{
    /// <summary>
    /// Gets the operation identifier.
    /// </summary>
    string OperationId { get; }

    /// <summary>
    /// Gets the cancellation token for this operation.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Reports progress for a successful item.
    /// </summary>
    /// <param name="itemDescription">Description of the processed item.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReportSuccessAsync(string itemDescription);

    /// <summary>
    /// Reports progress for a failed item.
    /// </summary>
    /// <param name="itemDescription">Description of the failed item.</param>
    /// <param name="error">The error message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReportFailureAsync(string itemDescription, string error);

    /// <summary>
    /// Reports a custom progress update.
    /// </summary>
    /// <param name="message">The progress message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReportProgressAsync(string message);

    /// <summary>
    /// Completes the operation.
    /// </summary>
    /// <param name="summary">Optional summary message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompleteAsync(string? summary = null);
}

/// <summary>
/// Represents the progress of a bulk operation.
/// </summary>
public class BulkOperationProgress
{
    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID performing the operation.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the total number of items to process.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the number of processed items.
    /// </summary>
    public int ProcessedItems { get; set; }

    /// <summary>
    /// Gets or sets the number of successful items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed items.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the current status message.
    /// </summary>
    public string CurrentMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the operation is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether the operation was cancelled.
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Gets or sets the start time of the operation.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the operation.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage => TotalItems > 0 ? (ProcessedItems * 100) / TotalItems : 0;

    /// <summary>
    /// Gets the estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (ProcessedItems == 0 || IsCompleted || IsCancelled)
            {
                return null;
            }

            var elapsed = DateTime.UtcNow - StartTime;
            var itemsPerSecond = ProcessedItems / elapsed.TotalSeconds;
            if (itemsPerSecond <= 0)
            {
                return null;
            }

            var remainingItems = TotalItems - ProcessedItems;
            var remainingSeconds = remainingItems / itemsPerSecond;
            return TimeSpan.FromSeconds(remainingSeconds);
        }
    }

    /// <summary>
    /// Gets or sets the list of error messages.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
