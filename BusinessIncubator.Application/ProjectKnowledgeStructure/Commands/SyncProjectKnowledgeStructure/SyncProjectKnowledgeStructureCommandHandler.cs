using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.BusinessIncubator.Domain.ValueObjects;
using LinaSys.BusinessIncubator.Infrastructure.Persistence;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectKnowledgeStructure;

/// <summary>
/// Handler for SyncProjectKnowledgeStructureCommand.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SyncProjectKnowledgeStructureCommandHandler"/> class.
/// </remarks>
public class SyncProjectKnowledgeStructureCommandHandler(
    IBusinessIncubatorRepository repository,
    BusinessIncubatorDbContext dbContext,
    ILogger<SyncProjectKnowledgeStructureCommandHandler> logger) : BaseCommandHandler<SyncProjectKnowledgeStructureCommand, SyncResult>
{

    /// <inheritdoc/>
    public override async Task<Result<SyncResult>> Handle(
        SyncProjectKnowledgeStructureCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting sync for Project: {ProjectId}, Preview: {Preview}",
            request.ProjectId,
            request.Preview);

        // Load the project with its knowledge structures
        var project = await repository.GetProjectByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found.");
        }

        var aggregatedResult = new SyncResult();

        // Process the project's knowledge structure if it exists
        var knowledgeStructure = project.GetKnowledgeStructure();
        if (knowledgeStructure is not null && knowledgeStructure.SourceKnowledgeStructureId.HasValue)
        {
            // Load source data
            var sourceData = await LoadSourceDataAsync(
                knowledgeStructure.SourceKnowledgeStructureId.Value,
                cancellationToken);

            // Perform sync
            var syncResult = knowledgeStructure.SyncFromSources(
                null,
                sourceData.SourceModules,
                sourceData.SourceTopics,
                sourceData.SourceSubjects,
                sourceData.SourceQuestions);

            aggregatedResult.Merge(syncResult);

            logger.LogInformation(
                "Synced KnowledgeStructure: {StructureId} with {ChangeCount} changes",
                knowledgeStructure.Id,
                syncResult.TotalChanges);
        }

        // Apply changes if not preview
        if (!request.Preview && aggregatedResult.HasChanges)
        {
            repository.Update(project);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Applied {TotalChanges} changes to Project: {ProjectId}",
                aggregatedResult.TotalChanges,
                request.ProjectId);
        }
        else if (request.Preview)
        {
            logger.LogInformation(
                "Preview mode - found {TotalChanges} potential changes for Project: {ProjectId}",
                aggregatedResult.TotalChanges,
                request.ProjectId);
        }

        return Success(aggregatedResult);
    }

    private Task<SourceDataContainer> LoadSourceDataAsync(
        long sourceStructureId,
        CancellationToken cancellationToken)
    {
        // This would load actual source data from the appropriate repositories
        // For now, returning empty container as placeholder
        logger.LogDebug("Loading source data for StructureId: {StructureId}", sourceStructureId);

        // TODO: Implement actual loading from KnowledgeStructure and Diagnostics repositories
        return Task.FromResult(new SourceDataContainer
        {
            SourceStructure = null, // Would be loaded from source repository
            SourceModules = [],
            SourceTopics = [],
            SourceSubjects = [],
            SourceQuestions = []
        });
    }

    private class SourceDataContainer
    {
        public LinaSys.KnowledgeStructure.Domain.Aggregates.KnowledgeStructure.KnowledgeStructure? SourceStructure { get; set; }
        public Dictionary<long, Module> SourceModules { get; set; } = [];
        public Dictionary<long, Topic> SourceTopics { get; set; } = [];
        public Dictionary<long, Subject> SourceSubjects { get; set; } = [];
        public Dictionary<long, Question> SourceQuestions { get; set; } = [];
    }
}