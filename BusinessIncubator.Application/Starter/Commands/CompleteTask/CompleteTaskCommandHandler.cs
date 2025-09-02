using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Core.Application.Dashboard.Services;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Starter.Commands.CompleteTask;

public class CompleteTaskCommandHandler(
    IStarterRepository starterRepository,
    IActivityTrackingService activityService,
    INotificationService notificationService,
    ITimeProvider timeProvider) : BaseCommandHandler<CompleteTaskCommand>
{
    public override async Task<Result> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the task
            var task = await starterRepository.GetTaskByIdAsync(request.TaskId);
            if (task is null)
            {
                return Failure(ResultErrorCodes.GenericError, ("TaskId", "Tarea no encontrada"));
            }

            // Verify ownership
            if (task.AssignedToUserId != request.UserId)
            {
                return Failure(ResultErrorCodes.Validation_SomeFieldsAreInvalid, ("Permission", "No tienes permisos para completar esta tarea"));
            }

            // Complete the task
            var now = timeProvider.UtcNow;
            task.Complete(now, request.CompletionNotes);

            // Update in repository
            await starterRepository.UpdateTaskAsync(task);

            // Get dashboard to update progress
            var dashboard = await starterRepository.GetStarterDashboardAsync(request.UserId, task.ProjectId);
            if (dashboard is not null)
            {
                dashboard.CompleteTask(request.TaskId, now);
                await starterRepository.UpdateDashboardAsync(dashboard);
            }

            await starterRepository.UnitOfWork.SaveEntitiesAsync();

            // Track activity
            await activityService.TrackActivityAsync(
                request.UserId,
                "task_completed",
                $"Completó la tarea: {task.Title}",
                "task",
                task.Id);

            // Create notification for mentor/coordinator if high priority
            if (task.Priority == "high" && task.AssignedBy is not null)
            {
                await notificationService.CreateNotificationAsync(
                    task.AssignedBy,
                    "Tarea Completada",
                    $"El participante ha completado la tarea: {task.Title}",
                    "task_completed",
                    "normal",
                    "tasks");
            }

            return Success();
        }
        catch (Exception ex)
        {
            return Failure(ResultErrorCodes.Unknown, ("Error", $"Error al completar la tarea: {ex.Message}"));
        }
    }
}