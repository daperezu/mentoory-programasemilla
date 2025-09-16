using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Public.Queries;

/// <summary>
/// Handler for retrieving detailed project information for public viewing.
/// </summary>
public class GetProjectDetailsQueryHandler(IBusinessIncubatorRepository repository) : BaseCommandHandler<GetProjectDetailsQuery, ProjectDetailDto>
{
    private readonly IBusinessIncubatorRepository _repository = repository;

    public override async Task<Result<ProjectDetailDto>> Handle(
        GetProjectDetailsQuery request,
        CancellationToken cancellationToken)
    {
        // Get project with stages - uses the method that accepts Guid (ExternalId)
        var project = await _repository.GetProjectWithStagesByExternalIdAsync(request.ExternalId, cancellationToken);

        if (project is null)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                ("Project", "El proyecto no fue encontrado"));
        }

        // Check if project is visible for public viewing
        if (project.Status != ProjectStatus.Active)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound, // Use existing error code
                ("Project", "El proyecto no está disponible para visualización pública"));
        }

        // Get the business incubator details
        var businessIncubator = await _repository.GetByIdAsync(project.BusinessIncubatorId, cancellationToken);

        if (businessIncubator is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                ("BusinessIncubator", "La incubadora de negocios no fue encontrada"));
        }

        // Map to DTO
        var dto = new ProjectDetailDto
        {
            ExternalId = project.ExternalId,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status.ToString(),
            BusinessIncubatorName = businessIncubator.Name,
            BusinessIncubatorDescription = businessIncubator.Description,
            BusinessIncubatorExternalId = businessIncubator.ExternalId,
            HeroImageBlobId = project.HeroImageBlobId,
            Latitude = project.Latitude,
            Longitude = project.Longitude,
            LocationName = project.LocationName,
            ActiveParticipants = project.ProjectUsers?.Count(u => u.IsActive) ?? 0,
            StartDate = project.ProjectStages?.OrderBy(s => s.StartDate).FirstOrDefault()?.StartDate,
            EndDate = project.ProjectStages?.OrderByDescending(s => s.EndDate).FirstOrDefault()?.EndDate,
            Stages = new List<ProjectStageDto>()
        };

        // Map stages if they exist
        if (project.ProjectStages != null && project.ProjectStages.Any())
        {
            var currentDate = DateTime.UtcNow;
            var currentStage = project.GetCurrentStage(currentDate);

            if (currentStage is not null)
            {
                dto.CurrentStageName = GetStageName(currentStage.Type);
            }

            dto.Stages = project.ProjectStages
                .OrderBy(s => s.StartDate)
                .Select(stage => new ProjectStageDto
                {
                    Title = GetStageName(stage.Type),
                    Description = stage.Description,
                    StartDate = stage.StartDate,
                    EndDate = stage.EndDate,
                    IsActive = stage.IsActive,
                    IsCurrent = stage.IsCurrent(currentDate),
                    Type = stage.Type.ToString()
                })
                .ToList();
        }

        // Generate hero image URL if needed
        if (string.IsNullOrWhiteSpace(dto.HeroImageUrl))
        {
            var encodedBlobId = !string.IsNullOrWhiteSpace(dto.HeroImageBlobId)
                ? Uri.EscapeDataString(dto.HeroImageBlobId)
                : "placeholder";
            dto.HeroImageUrl = $"/Public/Images/{encodedBlobId}?type=hero&text={Uri.EscapeDataString(dto.Name)}";
        }

        return Success(dto);
    }

    private static string GetStageName(ProjectStageType type)
    {
        return type switch
        {
            ProjectStageType.Invitation => "Invitación",
            ProjectStageType.InitialFormCollection => "Formularios Iniciales",
            ProjectStageType.Mentoring => "Mentoría",
            ProjectStageType.FinalFormCollection => "Formularios Finales",
            ProjectStageType.Closure => "Cierre",
            _ => type.ToString()
        };
    }
}