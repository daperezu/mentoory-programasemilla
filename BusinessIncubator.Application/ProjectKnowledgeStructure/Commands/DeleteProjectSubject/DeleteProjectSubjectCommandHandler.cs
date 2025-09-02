using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectSubject;

/// <summary>
/// Handler for deleting a project subject.
/// </summary>
public sealed class DeleteProjectSubjectCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectSubjectCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectSubjectCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectSubjectCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project
            var project = await repository.GetProjectByExternalIdAsync(
                request.ProjectExternalId,
                cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository
                .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

            if (knowledgeStructure is null)
            {
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                    ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));
            }

            // Find the subject and its parent topic
            ProjectSubject? subject = null;
            ProjectTopic? parentTopic = null;

            foreach (var module in knowledgeStructure.ProjectModules)
            {
                foreach (var topic in module.ProjectTopics)
                {
                    subject = topic.ProjectSubjects.FirstOrDefault(s => s.Id == request.SubjectId);
                    if (subject is not null)
                    {
                        parentTopic = topic;
                        break;
                    }
                }

                if (subject is not null)
                {
                    break;
                }
            }

            if (subject is null || parentTopic is null)
            {
                return Failure(ResultErrorCodes.Subject_NotFound,
                    (nameof(request.SubjectId), "Materia no encontrada"));
            }

            // Remove the subject from the topic
            parentTopic.RemoveProjectSubject(request.SubjectId);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted project subject {SubjectId} from project {ProjectId}",
                request.SubjectId,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting project subject {SubjectId} for project {ProjectId}",
                request.SubjectId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("DeleteSubject", "Error al eliminar la materia"));
        }
    }
}