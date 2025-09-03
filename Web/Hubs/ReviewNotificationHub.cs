using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinaSys.Web.Hubs;

/// <summary>
/// SignalR hub for review notifications.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReviewNotificationHub"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
[Authorize]
public class ReviewNotificationHub(ILogger<ReviewNotificationHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> ProjectGroups = new();

    /// <summary>
    /// Gets the list of connection IDs for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A set of connection IDs.</returns>
    public static HashSet<string> GetUserConnections(string userId)
    {
        return UserConnections.TryGetValue(userId, out var connections)
            ? new HashSet<string>(connections)
            : [];
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            AddUserConnection(userId, Context.ConnectionId);
            logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
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
            await LeaveAllProjectGroups(Context.ConnectionId);
            logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Joins a project group for notifications.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task JoinProjectGroup(long projectId)
    {
        var groupName = GetProjectGroupName(projectId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        ProjectGroups.AddOrUpdate(groupName,
            [Context.ConnectionId],
            (key, connections) =>
            {
                connections.Add(Context.ConnectionId);
                return connections;
            });

        logger.LogInformation("Connection {ConnectionId} joined project group {ProjectId}", Context.ConnectionId, projectId);
    }

    /// <summary>
    /// Leaves a project group.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LeaveProjectGroup(long projectId)
    {
        var groupName = GetProjectGroupName(projectId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        if (ProjectGroups.TryGetValue(groupName, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
            {
                ProjectGroups.TryRemove(groupName, out _);
            }
        }

        logger.LogInformation("Connection {ConnectionId} left project group {ProjectId}", Context.ConnectionId, projectId);
    }

    private static void AddUserConnection(string userId, string connectionId)
    {
        UserConnections.AddOrUpdate(userId,
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

    private static string GetProjectGroupName(long projectId) => $"project-{projectId}";

    private async Task LeaveAllProjectGroups(string connectionId)
    {
        var groupsToLeave = new List<string>();

        foreach (var kvp in ProjectGroups)
        {
            if (kvp.Value.Contains(connectionId))
            {
                kvp.Value.Remove(connectionId);
                groupsToLeave.Add(kvp.Key);

                if (kvp.Value.Count == 0)
                {
                    ProjectGroups.TryRemove(kvp.Key, out _);
                }
            }
        }

        foreach (var groupName in groupsToLeave)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }
    }
}
