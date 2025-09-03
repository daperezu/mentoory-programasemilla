using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectTopic;

/// <summary>
/// Handler for deleting a project topic.
/// </summary>
public sealed class DeleteProjectTopicCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectTopicCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectTopicCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectTopicCommand request,
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

            // Find the topic and its parent module
            ProjectTopic? topic = null;
            ProjectModule? parentModule = null;

            foreach (var module in knowledgeStructure.ProjectModules)
            {
                topic = module.ProjectTopics.FirstOrDefault(t => t.Id == request.TopicId);
                if (topic is not null)
                {
                    parentModule = module;
                    break;
                }
            }

            if (topic is null || parentModule is null)
            {
                return Failure(ResultErrorCodes.Topic_NotFound,
                    (nameof(request.TopicId), "Tema no encontrado"));
            }

            // Check if topic has subjects
            if (topic.ProjectSubjects.Any())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Topic", "No se puede eliminar un tema que contiene materias"));
            }

            // Handle questions linked to this topic
            // Get all project blocks to find questions linked to this topic
            var projectWithBlocks = await repository.GetProjectWithBlocksByExternalIdAsync(
                request.ProjectExternalId,
                cancellationToken);

            if (projectWithBlocks is not null)
            {
                // Find all questions linked to this topic and null out their TopicId
                var questionsToUpdate = projectWithBlocks.ProjectBlocks
                    .SelectMany(b => b.ProjectQuestions)
                    .Where(q => q.ProjectTopicId == topic.Id)
                    .ToList();

                foreach (var question in questionsToUpdate)
                {
                    question.UpdateTopicId(null);
                    logger.LogInformation(
                        "Removed topic reference from question {QuestionId}",
                        question.Id);
                }
            }

            // Remove the topic
            parentModule.RemoveProjectTopic(topic.Id);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted project topic {TopicId} from project {ProjectId}",
                request.TopicId,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting project topic {TopicId} for project {ProjectId}",
                request.TopicId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("DeleteTopic", "Error al eliminar el tema"));
        }
    }
}