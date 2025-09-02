using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectTopic;

/// <summary>
/// Handler for synchronizing a project topic with its source.
/// </summary>
internal sealed class SyncProjectTopicCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SyncProjectTopicCommandHandler> logger)
    : BaseCommandHandler<SyncProjectTopicCommand>
{
    public override async Task<Result> Handle(
        SyncProjectTopicCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the business incubator with project and knowledge structure
            var incubator = await repository.GetWithProjectAndKnowledgeStructureByExternalId(
                request.BusinessIncubatorExternalId,
                request.ProjectExternalId,
                cancellationToken);

            if (incubator is null)
            {
                return Failure(ResultErrorCodes.BusinessIncubator_NotFound,
                    (nameof(request.BusinessIncubatorExternalId), "Incubadora no encontrada"));
            }

            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);
            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
                project.Id, cancellationToken);

            if (knowledgeStructure is null)
            {
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                    ("ProjectKnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));
            }

            // Find the topic
            var topic = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .FirstOrDefault(t => t.Id == request.TopicId);

            if (topic is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.TopicId), "Tema no encontrado"));
            }

            if (!topic.SourceTopicId.HasValue)
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("SourceTopicId", "El tema no tiene origen para sincronizar"));
            }

            if (topic.IsFullyCustomized())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Topic", "El tema está completamente personalizado y no puede sincronizarse"));
            }

            // TODO: Implement actual sync logic
            // This would require:
            // 1. Determine if source is from global KnowledgeStructure or another project
            // 2. Fetch source topic data using appropriate queries
            // 3. Update non-customized fields in the topic
            // 4. Sync subjects within the topic if they are not customized
            // For now, log the operation
            logger.LogInformation(
                "Sincronización de tema solicitada para proyecto {ProjectId}, tema {TopicId}",
                project.Id,
                request.TopicId);

            repository.Update(incubator);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al sincronizar tema {TopicId} del proyecto {ProjectExternalId}",
                request.TopicId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("SyncTopic", "Error al sincronizar el tema"));
        }
    }
}