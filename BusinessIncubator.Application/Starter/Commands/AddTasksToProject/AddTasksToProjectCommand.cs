using LinaSys.BusinessIncubator.Domain.Aggregates.Starter;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Starter.Commands.AddTasksToProject;

public sealed record AddTasksToProjectCommand(
    string UserId,
    long ProjectId,
    List<ProjectTaskDto> Tasks) : IBaseRequest;

public sealed record ProjectTaskDto(
    string Title,
    string Description,
    string Category,
    int Order,
    DateTime? DueDate);

public sealed class AddTasksToProjectCommandHandler(
    IStarterRepository starterRepository,
    ITimeProvider timeProvider) : BaseCommandHandler<AddTasksToProjectCommand>
{
    public override async Task<Result> Handle(
        AddTasksToProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Get starter dashboard for user
        var dashboard = await starterRepository.GetStarterDashboardAsync(request.UserId, request.ProjectId);
        if (dashboard is null)
        {
            return Failure(ResultErrorCodes.User_NotFound, ("UserId", "Panel de inicio no encontrado para el usuario"));
        }

        // Add project-specific tasks
        foreach (var taskDto in request.Tasks)
        {
            var taskType = taskDto.Category switch
            {
                "profile" => "profile",
                "form" => "form",
                "training" => "training",
                _ => "general"
            };

            var task = new StarterTask(
                request.ProjectId,
                request.UserId,
                taskDto.Title,
                taskDto.Description,
                timeProvider.UtcNow,
                taskType,
                "normal", // priority
                taskDto.DueDate,
                null, // assignedBy
                taskDto.Category);

            dashboard.AddTask(task);
        }

        await starterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Success();
    }
}