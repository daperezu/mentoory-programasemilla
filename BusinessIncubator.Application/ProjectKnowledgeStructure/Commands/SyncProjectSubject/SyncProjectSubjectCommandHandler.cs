using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.SyncProjectSubject;

/// <summary>
/// Handler for synchronizing a project subject with its source.
/// </summary>
internal sealed class SyncProjectSubjectCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<SyncProjectSubjectCommandHandler> logger)
    : BaseCommandHandler<SyncProjectSubjectCommand>
{
    public override async Task<Result> Handle(
        SyncProjectSubjectCommand request,
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

            // Find the subject
            var subject = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .SelectMany(t => t.ProjectSubjects)
                .FirstOrDefault(s => s.Id == request.SubjectId);

            if (subject is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.SubjectId), "Materia no encontrada"));
            }

            if (!subject.SourceSubjectId.HasValue)
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("SourceSubjectId", "La materia no tiene origen para sincronizar"));
            }

            if (subject.IsFullyCustomized())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Subject", "La materia está completamente personalizada y no puede sincronizarse"));
            }

            // TODO: Implement actual sync logic
            // This would require:
            // 1. Determine if source is from global KnowledgeStructure or another project
            // 2. Fetch source subject data using appropriate queries
            // 3. Update non-customized fields in the subject
            // 4. Sync resources within the subject if they are not customized
            // For now, log the operation
            logger.LogInformation(
                "Sincronización de materia solicitada para proyecto {ProjectId}, materia {SubjectId}",
                project.Id,
                request.SubjectId);

            repository.Update(incubator);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al sincronizar materia {SubjectId} del proyecto {ProjectExternalId}",
                request.SubjectId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("SyncSubject", "Error al sincronizar la materia"));
        }
    }
}