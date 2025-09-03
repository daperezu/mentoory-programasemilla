using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectSubject;

/// <summary>
/// Handler for updating a project subject.
/// </summary>
public sealed class UpdateProjectSubjectCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<UpdateProjectSubjectCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectSubjectCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectSubjectCommand request,
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

            // Find the subject
            var subject = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .SelectMany(t => t.ProjectSubjects)
                .FirstOrDefault(s => s.Id == request.SubjectId);

            if (subject is null)
            {
                return Failure(ResultErrorCodes.Subject_NotFound,
                    (nameof(request.SubjectId), "Materia no encontrada"));
            }

            // Get the parent topic to check for duplicate names
            var parentTopic = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .FirstOrDefault(t => t.ProjectSubjects.Any(s => s.Id == request.SubjectId));

            if (parentTopic is not null &&
                parentTopic.ProjectSubjects
                    .Any(s => s.Id != request.SubjectId &&
                              s.Title.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe otra materia con este nombre en el tema"));
            }

            // Update only the fields that have changed
            if (subject.Title != request.Name)
            {
                subject.UpdateTitle(request.Name, isCustomized: true);
            }

            if (subject.Content != request.Content)
            {
                subject.UpdateContent(request.Content ?? string.Empty, isCustomized: true);
            }

            if (subject.Order != request.Order)
            {
                subject.UpdateOrder(request.Order, isCustomized: true);
            }

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated project subject {SubjectId} in project {ProjectId}",
                subject.Id,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating project subject {SubjectId} for project {ProjectId}",
                request.SubjectId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("UpdateSubject", "Error al actualizar la materia"));
        }
    }
}