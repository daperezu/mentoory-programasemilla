using FluentValidation;
using LinaSys.BusinessIncubator.Application.Project.DTOs;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.AddProjectStage;

/// <summary>
/// Command to add a new stage to a project.
/// </summary>
public sealed record AddProjectStageCommand(
    Guid ProjectExternalId,
    ProjectStageType Type,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate) : IBaseRequest<ProjectStageDto>;

/// <summary>
/// Validator for AddProjectStageCommand.
/// </summary>
public class AddProjectStageCommandValidator : AbstractValidator<AddProjectStageCommand>
{
    public AddProjectStageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("El tipo de etapa no es válido.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("El título es requerido.")
            .MaximumLength(200)
            .WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La descripción no puede exceder 2000 caracteres.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
    }
}

/// <summary>
/// Handler for AddProjectStageCommand.
/// </summary>
public class AddProjectStageCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    ITimeProvider timeProvider)
    : BaseCommandHandler<AddProjectStageCommand, ProjectStageDto>
{
    public override async Task<Result<ProjectStageDto>> Handle(
        AddProjectStageCommand request,
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

        try
        {
            // Add the stage through the aggregate
            var stage = project.AddStage(
                request.Type,
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                auditContext);

            // Save changes
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

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
    }
}
