using FluentValidation;
using LinaSys.BusinessIncubator.Application.Project.DTOs;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application.TimeProvider;

namespace LinaSys.BusinessIncubator.Application.Project.Queries.GetProjectStages;

/// <summary>
/// Query to get all stages for a project.
/// </summary>
public sealed record GetProjectStagesQuery(
    Guid ProjectExternalId) : IBaseRequest<List<ProjectStageDto>>;

/// <summary>
/// Validator for GetProjectStagesQuery.
/// </summary>
public class GetProjectStagesQueryValidator : AbstractValidator<GetProjectStagesQuery>
{
    public GetProjectStagesQueryValidator()
    {
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");
    }
}

/// <summary>
/// Handler for GetProjectStagesQuery.
/// </summary>
public class GetProjectStagesQueryHandler(IBusinessIncubatorRepository repository, ITimeProvider timeProvider)
    : BaseCommandHandler<GetProjectStagesQuery, List<ProjectStageDto>>
{
    public override async Task<Result<List<ProjectStageDto>>> Handle(GetProjectStagesQuery request, CancellationToken cancellationToken)
    {
        // Get the project with stages
        var project = await repository.GetProjectWithStagesByExternalIdAsync(request.ProjectExternalId, cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
        }

        // Map stages to DTOs
        var currentDate = timeProvider.UtcNow;
        var stageDtos = project.GetStagesOrdered()
            .Select(stage => new ProjectStageDto
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
            })
            .ToList();

        return Success(stageDtos);
    }
}
