using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectTopic;

/// <summary>
/// Handler for creating a new project topic.
/// </summary>
public sealed class CreateProjectTopicCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<CreateProjectTopicCommandHandler> logger)
    : BaseCommandHandler<CreateProjectTopicCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectTopicCommand request,
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

            // Find the module
            var module = knowledgeStructure.ProjectModules
                .FirstOrDefault(m => m.Id == request.ModuleId);

            if (module is null)
            {
                return Failure(ResultErrorCodes.Module_NotFound,
                    (nameof(request.ModuleId), "Módulo no encontrado"));
            }

            // Check if a topic with the same name already exists in the module
            if (module.ProjectTopics
                .Any(t => t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe un tema con este nombre en el módulo"));
            }

            // Create the new topic
            var newTopic = module.AddProjectTopic(
                sourceTopicId: null,
                name: request.Name,
                isNameCustomized: true,
                order: request.Order,
                isOrderCustomized: true);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created project topic {TopicId} in module {ModuleId} for project {ProjectId}",
                newTopic.Id,
                module.Id,
                project.Id);

            return Success(newTopic.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating project topic for project {ProjectId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateTopic", "Error al crear el tema"));
        }
    }
}