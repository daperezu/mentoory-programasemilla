using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncAllProjectKnowledgeStructure;

/// <summary>
/// Handler for synchronizing all non-customized elements in a project's knowledge structure.
/// </summary>
internal sealed class SyncAllProjectKnowledgeStructureCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SyncAllProjectKnowledgeStructureCommandHandler> logger)
    : BaseCommandHandler<SyncAllProjectKnowledgeStructureCommand>
{
    public override async Task<Result> Handle(
        SyncAllProjectKnowledgeStructureCommand request,
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

            if (knowledgeStructure.SourceKnowledgeStructureId is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("SourceKnowledgeStructure", "La estructura no tiene origen para sincronizar"));
            }

            var syncedCount = 0;

            // TODO: Implement actual sync logic with source modules/topics/subjects
            // This is a complex cross-domain operation that requires:
            // 1. Determine the source type (Global KnowledgeStructure or another Project)
            // 2. For Global source:
            //    - Use mediator to send queries to KnowledgeStructure.Application
            //    - GetModuleDetailsQuery, GetTopicDetailsQuery, GetSubjectDetailsQuery
            //    - Update non-customized fields in project entities
            // 3. For Project source:
            //    - Fetch source project's knowledge structure from repository
            //    - Copy non-customized fields from source project entities
            // 4. Handle resources and questions sync as well
            // 5. Use domain methods like UpdateFromSource() on entities
            //
            // For now, this is a placeholder implementation that just counts elements
            // This should be implemented as part of a future enhancement

            // Count all non-customized elements that would be synced
            foreach (var module in knowledgeStructure.ProjectModules.Where(m => !m.IsFullyCustomized()))
            {
                if (module.SourceModuleId.HasValue)
                {
                    syncedCount++;

                    // Count topics within the module
                    foreach (var topic in module.ProjectTopics.Where(t => !t.IsFullyCustomized()))
                    {
                        if (topic.SourceTopicId.HasValue)
                        {
                            syncedCount++;

                            // Count subjects within the topic
                            foreach (var subject in topic.ProjectSubjects.Where(s => !s.IsFullyCustomized()))
                            {
                                if (subject.SourceSubjectId.HasValue)
                                {
                                    syncedCount++;
                                }
                            }
                        }
                    }
                }
            }

            repository.Update(incubator);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Sincronización completa para proyecto {ProjectId}. Elementos sincronizados: {SyncedCount}",
                project.Id,
                syncedCount);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al sincronizar estructura del proyecto {ProjectExternalId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("SyncAll", "Error al sincronizar la estructura"));
        }
    }
}