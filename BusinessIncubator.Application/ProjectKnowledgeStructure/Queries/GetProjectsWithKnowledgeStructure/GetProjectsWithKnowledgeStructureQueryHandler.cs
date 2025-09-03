using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Queries.GetProjectsWithKnowledgeStructure;

/// <summary>
/// Handler for GetProjectsWithKnowledgeStructureQuery.
/// </summary>
public sealed partial class GetProjectsWithKnowledgeStructureQueryHandler(
    IBusinessIncubatorRepository repository,
    ILogger<GetProjectsWithKnowledgeStructureQueryHandler> logger)
    : BaseCommandHandler<GetProjectsWithKnowledgeStructureQuery, List<ProjectWithKnowledgeStructureDto>>
{
    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public override async Task<Result<List<ProjectWithKnowledgeStructureDto>>> Handle(
        GetProjectsWithKnowledgeStructureQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var incubator = await repository.GetByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);
            if (incubator is null)
            {
                return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), request.BusinessIncubatorExternalId.ToString()));
            }

            // Get all projects from the incubator that have knowledge structures
            var projects = await repository.GetProjectsWithKnowledgeStructureAsync(incubator.Id, request.ExcludeProjectExternalId, cancellationToken);

            // For each project, get its knowledge structure details
            var dtos = new List<ProjectWithKnowledgeStructureDto>();
            foreach (var project in projects)
            {
                var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);
                if (knowledgeStructure is not null)
                {
                    var dto = new ProjectWithKnowledgeStructureDto
                    {
                        Id = project.Id,
                        ExternalId = project.ExternalId,
                        Name = project.Name,
                        Key = project.Key,
                        KnowledgeStructureName = knowledgeStructure.Name,
                        ModuleCount = knowledgeStructure.ProjectModules.Count,
                        TopicCount = knowledgeStructure.ProjectModules
                            .SelectMany(m => m.ProjectTopics)
                            .Count(),
                        SubjectCount = knowledgeStructure.ProjectModules
                            .SelectMany(m => m.ProjectTopics)
                            .SelectMany(t => t.ProjectSubjects)
                            .Count()
                    };
                    dtos.Add(dto);
                }
            }

            return Success(dtos);
        }
        catch (Exception ex)
        {
            LogQueryFailed(ex.Message);
            return Failure(ResultErrorCodes.Unknown, ("Error", ex.Message));
        }
    }

    [LoggerMessage(EventId = 4011, Level = LogLevel.Error, Message = "Query failed: {Message}")]
    private partial void LogQueryFailed(string message);
}