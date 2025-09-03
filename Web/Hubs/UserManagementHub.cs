using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinaSys.Web.Hubs;

/// <summary>
/// SignalR hub for user management real-time notifications.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserManagementHub"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
[Authorize(Roles = "Coordinator,Administrator,GlobalAdministrator")]
public class UserManagementHub(ILogger<UserManagementHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
    private static readonly ConcurrentDictionary<string, BulkOperationProgress> BulkOperations = new();

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            AddUserConnection(userId, Context.ConnectionId);
            logger.LogInformation("User {UserId} connected to UserManagementHub with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

            // Send any pending bulk operation progress
            if (BulkOperations.TryGetValue(userId, out var progress))
            {
                await Clients.Caller.SendAsync("BulkOperationProgress", progress);
            }
        }

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            RemoveUserConnection(userId, Context.ConnectionId);
            logger.LogInformation("User {UserId} disconnected from UserManagementHub with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Notifies all connected administrators when a new user is created.
    /// </summary>
    /// <param name="userId">The new user ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="role">The user's role.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyUserCreated(string userId, string email, string role)
    {
        logger.LogInformation("Broadcasting user created: {UserId} - {Email} - {Role}", userId, email, role);
        await Clients.All.SendAsync("UserCreated", new { userId, email, role, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Notifies all connected administrators when a user is updated.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="changes">Dictionary of changed properties.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyUserUpdated(string userId, Dictionary<string, object> changes)
    {
        logger.LogInformation("Broadcasting user updated: {UserId} with {ChangeCount} changes", userId, changes.Count);
        await Clients.All.SendAsync("UserUpdated", new { userId, changes, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Updates bulk operation progress for a specific batch.
    /// </summary>
    /// <param name="batchId">The batch operation ID.</param>
    /// <param name="progress">The current progress percentage (0-100).</param>
    /// <param name="message">Status message.</param>
    /// <param name="currentItem">Current item being processed.</param>
    /// <param name="totalItems">Total items to process.</param>
    /// <param name="successCount">Number of successful items.</param>
    /// <param name="failureCount">Number of failed items.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyBulkOperationProgress(
        string batchId,
        int progress,
        string message,
        int currentItem,
        int totalItems,
        int successCount,
        int failureCount)
    {
        var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var progressData = new BulkOperationProgress
        {
            BatchId = batchId,
            Progress = progress,
            Message = message,
            CurrentItem = currentItem,
            TotalItems = totalItems,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Timestamp = DateTime.UtcNow,
        };

        // Store progress for reconnection scenarios
        BulkOperations.AddOrUpdate(userId, progressData, (key, existing) => progressData);

        // Send to the calling user
        await Clients.Caller.SendAsync("BulkOperationProgress", progressData);

        // Clean up completed operations
        if (progress >= 100)
        {
            _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(t =>
            {
                BulkOperations.TryRemove(userId, out _);
            });
        }

        logger.LogInformation(
            "Bulk operation {BatchId} progress: {Progress}% - {CurrentItem}/{TotalItems} - {Message}",
            batchId,
            progress,
            currentItem,
            totalItems,
            message);
    }

    /// <summary>
    /// Notifies all connected administrators when a user's status changes.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="isActive">The new active status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyUserStatusChanged(string userId, bool isActive)
    {
        logger.LogInformation("Broadcasting user status changed: {UserId} - Active: {IsActive}", userId, isActive);
        await Clients.All.SendAsync("UserStatusChanged", new { userId, isActive, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Subscribes to updates for a specific incubator.
    /// </summary>
    /// <param name="incubatorId">The incubator ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SubscribeToIncubator(long incubatorId)
    {
        var groupName = GetIncubatorGroupName(incubatorId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("Connection {ConnectionId} subscribed to incubator {IncubatorId}", Context.ConnectionId, incubatorId);
    }

    /// <summary>
    /// Unsubscribes from updates for a specific incubator.
    /// </summary>
    /// <param name="incubatorId">The incubator ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UnsubscribeFromIncubator(long incubatorId)
    {
        var groupName = GetIncubatorGroupName(incubatorId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("Connection {ConnectionId} unsubscribed from incubator {IncubatorId}", Context.ConnectionId, incubatorId);
    }

    private static void AddUserConnection(string userId, string connectionId)
    {
        UserConnections.AddOrUpdate(
            userId,
            [connectionId],
            (key, connections) =>
            {
                connections.Add(connectionId);
                return connections;
            });
    }

    private static void RemoveUserConnection(string userId, string connectionId)
    {
        if (UserConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                UserConnections.TryRemove(userId, out _);
            }
        }
    }

    private static string GetIncubatorGroupName(long incubatorId) => $"incubator-{incubatorId}";
}

/// <summary>
/// Represents the progress of a bulk operation.
/// </summary>
public class BulkOperationProgress
{
    /// <summary>
    /// Gets or sets the batch operation ID.
    /// </summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current item being processed.
    /// </summary>
    public int CurrentItem { get; set; }

    /// <summary>
    /// Gets or sets the total items to process.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the number of successful items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed items.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the progress update.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
