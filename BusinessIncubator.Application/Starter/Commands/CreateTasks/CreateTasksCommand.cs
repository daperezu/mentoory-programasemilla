using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Starter.Commands.CreateTasks;

public sealed record CreateTasksCommand(
    string UserId,
    long ProjectId,
    List<TaskDto> Tasks) : IBaseRequest;

public sealed record TaskDto(
    string Title,
    string Description,
    string TaskType,
    int Order,
    DateTime? DueDate);

public sealed class CreateTasksCommandHandler(
    IStarterRepository starterRepository,
    ITimeProvider timeProvider) : BaseCommandHandler<CreateTasksCommand>
{
    public override async Task<Result> Handle(
        CreateTasksCommand request,
        CancellationToken cancellationToken)
    {
        // Get or create starter dashboard for user
        var now = timeProvider.UtcNow;
        var dashboard = await starterRepository.GetStarterDashboardAsync(request.UserId, request.ProjectId);

        if (dashboard is null)
        {
            dashboard = new StarterDashboard(request.UserId, "participant", request.ProjectId, now);
            await starterRepository.AddDashboardAsync(dashboard);
        }

        // Add tasks to dashboard
        foreach (var taskDto in request.Tasks)
        {
            var task = new StarterTask(
                request.ProjectId,
                request.UserId,
                taskDto.Title,
                taskDto.Description,
                now,
                taskDto.TaskType,
                "normal", // priority
                taskDto.DueDate);

            dashboard.AddTask(task);
        }

        // Save changes
        await starterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Success();
    }
}