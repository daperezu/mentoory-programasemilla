using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectQuestion;

/// <summary>
/// Handler for updating a project question.
/// </summary>
public sealed class UpdateProjectQuestionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<UpdateProjectQuestionCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectQuestionCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectQuestionCommand request,
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

            // Find the question in project blocks
            var question = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions)
                .FirstOrDefault(q => q.Id == request.QuestionId);

            if (question is null)
            {
                return Failure(ResultErrorCodes.Question_NotFound,
                    (nameof(request.QuestionId), "Pregunta no encontrada"));
            }

            // Update only the fields that have changed
            // For text: only mark as customized if it changed
            if (question.Text != request.Text)
            {
                question.UpdateText(request.Text, isCustomized: true);
            }

            // For answer type: only mark as customized if it changed
            if (question.AnswerType != request.AnswerType)
            {
                question.UpdateAnswerType(request.AnswerType, isCustomized: true);
            }

            // For phase: only mark as customized if it changed
            if (question.AppliesToPhase != request.AppliesToPhase)
            {
                question.UpdateAppliesToPhase(request.AppliesToPhase, isCustomized: true);
            }

            // For diagnosis flag: only mark as customized if it changed
            if (question.IsUsedForDiagnosis != request.IsUsedForDiagnosis)
            {
                question.UpdateUsedForDiagnosis(request.IsUsedForDiagnosis, isCustomized: true);
            }

            // For order: always update but don't mark as customized since order is not synced
            if (question.Order != request.Order)
            {
                question.UpdateOrder(request.Order, isCustomized: false);
            }

            // Handle topic assignment
            if (request.TopicId.HasValue)
            {
                // Verify topic exists in the project knowledge structure
                var knowledgeStructure = await repository.GetProjectKnowledgeStructureAsync(
                    project.Id,
                    cancellationToken);

                if (knowledgeStructure is not null)
                {
                    var topicExists = knowledgeStructure.ProjectModules
                        .SelectMany(m => m.ProjectTopics)
                        .Any(t => t.Id == request.TopicId.Value);

                    if (!topicExists)
                    {
                        return Failure(ResultErrorCodes.Topic_NotFound,
                            (nameof(request.TopicId), "Tema no encontrado en la estructura de conocimiento"));
                    }
                }

                // Update the topic ID
                question.UpdateTopicId(request.TopicId.Value);
            }
            else
            {
                // Remove topic assignment if null
                question.UpdateTopicId(null);
            }

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated project question {QuestionId} in project {ProjectId}",
                question.Id,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating project question {QuestionId} for project {ProjectId}",
                request.QuestionId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("UpdateQuestion", "Error al actualizar la pregunta"));
        }
    }
}
