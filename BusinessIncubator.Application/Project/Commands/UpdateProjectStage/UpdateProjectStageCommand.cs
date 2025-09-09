using FluentValidation;
using LinaSys.Auth.Application.Queries;
using LinaSys.BusinessIncubator.Application.Project.DTOs;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.IntegrationEvents.BusinessIncubator;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.Constants;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectStage;

/// <summary>
/// Command to update an existing project stage.
/// </summary>
public sealed record UpdateProjectStageCommand(
    Guid ProjectExternalId,
    ProjectStageType Type,
    string? Title,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsActive) : IBaseRequest<ProjectStageDto>;

/// <summary>
/// Validator for UpdateProjectStageCommand.
/// </summary>
public class UpdateProjectStageCommandValidator : AbstractValidator<UpdateProjectStageCommand>
{
    public UpdateProjectStageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("El tipo de etapa no es válido.");

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("El título no puede estar vacío.")
                .MaximumLength(200)
                .WithMessage("El título no puede exceder 200 caracteres.");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("La descripción no puede exceder 2000 caracteres.");
        });

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.StartDate < x.EndDate)
                .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
        });
    }
}

/// <summary>
/// Handler for UpdateProjectStageCommand.
/// </summary>
public class UpdateProjectStageCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    ITimeProvider timeProvider,
    IPublisher publisher,
    IMediator mediator)
    : BaseCommandHandler<UpdateProjectStageCommand, ProjectStageDto>
{
    public override async Task<Result<ProjectStageDto>> Handle(
        UpdateProjectStageCommand request,
        CancellationToken cancellationToken)
    {
        // Get the project with stages
        var project = await repository.GetProjectWithStagesByExternalIdAsync(
            request.ProjectExternalId,
            cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
        }

        var stage = project.GetStage(request.Type);
        if (stage is null)
        {
            return Failure(
                ResultErrorCodes.Project_UpdateFailed,
                ("Stage", $"No existe una etapa de tipo {request.Type} en este proyecto."));
        }

        // Store previous active state to detect activation
        var wasActive = stage.IsActive;

        try
        {
            // Update stage details if provided
            if (request.Title != null || request.Description != null)
            {
                project.UpdateStageDetails(request.Type, request.Title, request.Description, auditContext);
            }

            // Update stage dates if provided
            if (request is { StartDate: not null, EndDate: not null })
            {
                project.UpdateStageDates(request.Type, request.StartDate.Value, request.EndDate.Value, auditContext);
            }

            // Update active status if provided
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    project.ActivateStage(request.Type, auditContext);
                }
                else
                {
                    project.DeactivateStage(request.Type, auditContext);
                }
            }

            // Save changes
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Get updated stage for DTO
            stage = project.GetStage(request.Type)!;

            // Fire integration event if stage was activated and it's a form collection stage
            if (!wasActive && stage is { IsActive: true, Type: ProjectStageType.InitialFormCollection or ProjectStageType.FinalFormCollection })
            {
                var phaseEnum = stage.Type == ProjectStageType.InitialFormCollection
                    ? QuestionPhase.Start
                    : QuestionPhase.Final;

                // Get project with users
                var projectWithUsers = await repository.GetProjectWithUsersAsync(project.Id, cancellationToken);
                if (projectWithUsers?.ProjectUsers != null)
                {
                    var participants = new List<ParticipantNotificationInfo>();

                    // Get details for each Starter participant
                    foreach (var projectUser in projectWithUsers.ProjectUsers.Where(pu => pu is { IsActive: true, Role: Roles.Starter }))
                    {
                        var userResult = await mediator.Send(new GetUserByIdQuery(projectUser.UserId), cancellationToken);
                        if (userResult is { IsSuccess: true, Value: not null })
                        {
                            participants.Add(new ParticipantNotificationInfo(
                                UserId: projectUser.UserId,
                                Email: userResult.Value.Email,
                                FullName: userResult.Value.FullName ?? userResult.Value.UserName));
                        }
                    }

                    if (participants.Count > 0)
                    {
                        var integrationEvent = new ProjectStageActivatedIntegrationEvent(
                            ProjectId: project.Id,
                            ProjectExternalId: project.ExternalId,
                            ProjectName: project.Name,
                            StageType: stage.Type.ToString(),
                            Phase: phaseEnum.ToString(),
                            StageName: stage.Title,
                            StartDate: stage.StartDate,
                            EndDate: stage.EndDate,
                            Participants: participants,
                            OccurredAt: timeProvider.UtcNow);

                        await publisher.Publish(integrationEvent, cancellationToken);
                    }
                }
            }

            // Map to DTO
            var currentDate = timeProvider.UtcNow;
            var dto = new ProjectStageDto
            {
                Id = stage.Id,
                ProjectId = stage.ProjectId,
                Type = stage.Type,
                Title = stage.Title,
                Description = stage.Description,
                StartDate = stage.StartDate,
                EndDate = stage.EndDate,
                IsActive = stage.IsActive,
                IsCurrent = stage.IsCurrent(currentDate),
                DaysRemaining = stage.GetDaysRemaining(currentDate),
                CreatedAt = stage.CreatedAt,
                CreatedBy = stage.CreatedBy ?? string.Empty,
                UpdatedAt = stage.UpdatedAt,
                UpdatedBy = stage.UpdatedBy
            };

            return Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(
                ResultErrorCodes.Project_UpdateFailed,
                ("Stage", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return Failure(
                ResultErrorCodes.Project_UpdateFailed,
                ("Validation", ex.Message));
        }
    }
}
