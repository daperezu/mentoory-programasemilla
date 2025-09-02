using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectSubject;

/// <summary>
/// Handler for creating a new project subject.
/// </summary>
public sealed class CreateProjectSubjectCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<CreateProjectSubjectCommandHandler> logger)
    : BaseCommandHandler<CreateProjectSubjectCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectSubjectCommand request,
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

            // Find the topic
            var topic = knowledgeStructure.ProjectModules
                .SelectMany(m => m.ProjectTopics)
                .FirstOrDefault(t => t.Id == request.TopicId);

            if (topic is null)
            {
                return Failure(ResultErrorCodes.Topic_NotFound,
                    (nameof(request.TopicId), "Tema no encontrado"));
            }

            // Check if a subject with the same name already exists in the topic
            if (topic.ProjectSubjects
                .Any(s => s.Title.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe una materia con este nombre en el tema"));
            }

            // Create the new subject
            var newSubject = topic.AddProjectSubject(
                sourceSubjectId: null,
                title: request.Name,
                isTitleCustomized: true,
                content: request.Content,
                isContentCustomized: !string.IsNullOrEmpty(request.Content),
                order: request.Order,
                isOrderCustomized: true);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created project subject {SubjectId} in topic {TopicId} for project {ProjectId}",
                newSubject.Id,
                topic.Id,
                project.Id);

            return Success(newSubject.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating project subject for project {ProjectId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateSubject", "Error al crear la materia"));
        }
    }
}