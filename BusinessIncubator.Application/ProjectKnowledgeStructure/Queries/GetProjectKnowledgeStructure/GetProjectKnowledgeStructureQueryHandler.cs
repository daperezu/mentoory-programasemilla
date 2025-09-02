using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectKnowledgeStructure;

/// <summary>
/// Handler for getting the project knowledge structure.
/// </summary>
public sealed class GetProjectKnowledgeStructureQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetProjectKnowledgeStructureQueryHandler> logger)
    : BaseCommandHandler<GetProjectKnowledgeStructureQuery, ProjectKnowledgeStructureDto>
{
    public override async Task<Result<ProjectKnowledgeStructureDto>> Handle(
        GetProjectKnowledgeStructureQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project directly
            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository
                .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

            if (knowledgeStructure is null)
            {
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound, (nameof(request.ProjectExternalId), "El proyecto no tiene estructura de conocimiento"));
            }

            // Map to DTO
            var dto = new ProjectKnowledgeStructureDto
            {
                Id = knowledgeStructure.Id,
                ProjectId = knowledgeStructure.ProjectId,
                ProjectName = project.Name,
                SourceKnowledgeStructureId = knowledgeStructure.SourceKnowledgeStructureId,
                Name = knowledgeStructure.Name,
                IsNameCustomized = knowledgeStructure.IsNameCustomized,
                Description = knowledgeStructure.Description,
                IsDescriptionCustomized = knowledgeStructure.IsDescriptionCustomized,
                Modules = knowledgeStructure.ProjectModules
                    .OrderBy(m => m.Order)
                    .Select(m => new ProjectModuleDto
                    {
                        Id = m.Id,
                        SourceModuleId = m.SourceModuleId,
                        Name = m.Name,
                        IsNameCustomized = m.IsNameCustomized,
                        Order = m.Order,
                        IsOrderCustomized = m.IsOrderCustomized,
                        Topics = m.ProjectTopics
                            .OrderBy(t => t.Order)
                            .Select(t => new ProjectTopicDto
                            {
                                Id = t.Id,
                                SourceTopicId = t.SourceTopicId,
                                Name = t.Name,
                                IsNameCustomized = t.IsNameCustomized,
                                Order = t.Order,
                                IsOrderCustomized = t.IsOrderCustomized,
                                Subjects = t.ProjectSubjects
                                    .OrderBy(s => s.Order)
                                    .Select(s => new ProjectSubjectDto
                                    {
                                        Id = s.Id,
                                        SourceSubjectId = s.SourceSubjectId,
                                        Title = s.Title,
                                        IsTitleCustomized = s.IsTitleCustomized,
                                        Content = s.Content,
                                        IsContentCustomized = s.IsContentCustomized,
                                        Order = s.Order,
                                        IsOrderCustomized = s.IsOrderCustomized,
                                        Resources = s.ProjectSubjectResources
                                            .OrderBy(r => r.Order)
                                            .Select(r => new ProjectSubjectResourceDto
                                            {
                                                Id = r.Id,
                                                SourceSubjectResourceId = r.SourceSubjectResourceId,
                                                Title = r.Title,
                                                IsTitleCustomized = r.IsTitleCustomized,
                                                Url = r.Url,
                                                IsUrlCustomized = r.IsUrlCustomized,
                                                Type = r.Type,
                                                IsTypeCustomized = r.IsTypeCustomized,
                                                EstimatedMinutes = r.EstimatedMinutes,
                                                IsEstimatedMinutesCustomized = r.IsEstimatedMinutesCustomized,
                                                Order = r.Order,
                                                IsOrderCustomized = r.IsOrderCustomized
                                            }).ToList()
                                    }).ToList()
                            }).ToList()
                    }).ToList()
            };

            return Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error getting project knowledge structure for project {ProjectExternalId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown, ("KnowledgeStructureGet", "Error al obtener la estructura de conocimiento"));
        }
    }
}
