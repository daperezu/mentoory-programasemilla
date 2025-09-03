using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Starter.Commands.CompleteTask;

public class CompleteTaskCommand(string userId, long taskId, string? completionNotes = null) : IBaseRequest
{
    public string UserId { get; } = userId;

    public long TaskId { get; } = taskId;

    public string? CompletionNotes { get; } = completionNotes;
}