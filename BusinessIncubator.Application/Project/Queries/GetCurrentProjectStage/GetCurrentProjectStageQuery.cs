using FluentValidation;
using LinaSys.BusinessIncubator.Application.Project.DTOs;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Project.Queries.GetCurrentProjectStage;

/// <summary>
/// Query to get the current active stage for a project.
/// </summary>
public sealed record GetCurrentProjectStageQuery(
    Guid ProjectExternalId) : IBaseRequest<ProjectStageDto?>;

/// <summary>
/// Validator for GetCurrentProjectStageQuery.
/// </summary>
public class GetCurrentProjectStageQueryValidator : AbstractValidator<GetCurrentProjectStageQuery>
{
    public GetCurrentProjectStageQueryValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");
    }
}

/// <summary>
/// Handler for GetCurrentProjectStageQuery.
/// </summary>
public class GetCurrentProjectStageQueryHandler(
    IBusinessIncubatorRepository repository,
    ITimeProvider timeProvider)
    : BaseCommandHandler<GetCurrentProjectStageQuery, ProjectStageDto?>
{
    public override async Task<Result<ProjectStageDto?>> Handle(GetCurrentProjectStageQuery request, CancellationToken cancellationToken)
    {
        // Get the project with stages
        var project = await repository.GetProjectWithStagesByExternalIdAsync(request.ProjectExternalId, cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
        }

        // Get the current stage
        var currentDate = timeProvider.UtcNow;
        var currentStage = project.GetCurrentStage(currentDate);

        if (currentStage is null)
        {
            // No current stage - return success with null
            return Success(null);
        }

        // Map to DTO
        var dto = new ProjectStageDto
        {
            Id = currentStage.Id,
            ProjectId = currentStage.ProjectId,
            Type = currentStage.Type,
            Title = currentStage.Title,
            Description = currentStage.Description,
            StartDate = currentStage.StartDate,
            EndDate = currentStage.EndDate,
            IsActive = currentStage.IsActive,
            IsCurrent = true, // It's the current stage
            DaysRemaining = currentStage.GetDaysRemaining(currentDate),
            CreatedAt = currentStage.CreatedAt,
            CreatedBy = currentStage.CreatedBy ?? string.Empty,
            UpdatedAt = currentStage.UpdatedAt,
            UpdatedBy = currentStage.UpdatedBy
        };

        return Success(dto);
    }
}
