using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectTopic;

/// <summary>
/// Handler for updating a project topic.
/// </summary>
public sealed class UpdateProjectTopicCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<UpdateProjectTopicCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectTopicCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectTopicCommand request,
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

            // Get the parent module to check for duplicate names
            var parentModule = knowledgeStructure.ProjectModules
                .FirstOrDefault(m => m.ProjectTopics.Any(t => t.Id == request.TopicId));

            if (parentModule is not null &&
                parentModule.ProjectTopics
                    .Any(t => t.Id != request.TopicId &&
                              t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe otro tema con este nombre en el módulo"));
            }

            // Update only the fields that have changed
            if (topic.Name != request.Name)
            {
                topic.UpdateName(request.Name, isCustomized: true);
            }

            if (topic.Order != request.Order)
            {
                topic.UpdateOrder(request.Order, isOrderCustomized: true);
            }

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated project topic {TopicId} in project {ProjectId}",
                topic.Id,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating project topic {TopicId} for project {ProjectId}",
                request.TopicId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("UpdateTopic", "Error al actualizar el tema"));
        }
    }
}