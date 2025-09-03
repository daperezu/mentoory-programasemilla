using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;

namespace LinaSys.BusinessIncubator.Application.Starter.Services;

public interface ITaskGenerationService
{
    Task GenerateTasksForPhaseAsync(string userId, long projectId, string phase);
    Task CheckAndGenerateOverdueTasksAsync(string userId, long projectId);
    Task GenerateTaskFromTemplateAsync(string userId, long projectId, string templateCode);
    Task<List<StarterTask>> GetPendingTasksAsync(string userId, long projectId);
    Task UpdateTaskStatusAsync(long taskId, string status, string? notes = null);
}