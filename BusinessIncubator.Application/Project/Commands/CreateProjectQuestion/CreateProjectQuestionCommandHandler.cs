using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectQuestion;

/// <summary>
/// Handler for creating a new project question.
/// </summary>
public sealed class CreateProjectQuestionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<CreateProjectQuestionCommandHandler> logger)
    : BaseCommandHandler<CreateProjectQuestionCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectQuestionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project with blocks
            var project = await repository.GetProjectWithBlocksByExternalIdAsync(
                request.ProjectExternalId,
                cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Find the block
            var block = project.ProjectBlocks.FirstOrDefault(b => b.Id == request.BlockId);
            if (block is null)
            {
                return Failure(ResultErrorCodes.Block_NotFound,
                    (nameof(request.BlockId), "Bloque no encontrado"));
            }

            // Create the question
            ProjectQuestion newQuestion;

            if (request.TopicId.HasValue)
            {
                // If TopicId is provided, add through topic
                // Need to load the project with knowledge structure to verify topic
                // Get the project knowledge structure
                var knowledgeStructure = await repository
                    .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

                if (knowledgeStructure is null)
                {
                    return Failure(ResultErrorCodes.KnowledgeStructure_NotFound,
                        ("KnowledgeStructure", "El proyecto no tiene estructura de conocimiento"));
                }

                var topic = knowledgeStructure.ProjectModules
                    .SelectMany(m => m.ProjectTopics)
                    .FirstOrDefault(t => t.Id == request.TopicId.Value);

                if (topic is null)
                {
                    return Failure(ResultErrorCodes.Topic_NotFound,
                        (nameof(request.TopicId), "Tema no encontrado"));
                }

                // Add question through topic
                newQuestion = topic.AddProjectQuestion(
                    projectBlockId: block.Id,
                    sourceQuestionId: null,
                    text: request.Text,
                    isTextCustomized: true,
                    answerType: request.AnswerType,
                    isAnswerTypeCustomized: true,
                    appliesToPhase: request.AppliesToPhase,
                    isAppliesToPhaseCustomized: true,
                    isUsedForMentoringPlan: false,
                    isMentoringPlanCustomized: true,
                    isUsedForDiagnosis: request.IsUsedForDiagnosis,
                    isDiagnosisCustomized: true,
                    order: request.Order,
                    isOrderCustomized: true);

                // Save the project
                repository.Update(project);
            }
            else
            {
                // Add question directly to block (no topic)
                newQuestion = block.AddProjectQuestion(
                    sourceQuestionId: null,
                    text: request.Text,
                    isTextCustomized: true,
                    answerType: request.AnswerType,
                    isAnswerTypeCustomized: true,
                    appliesToPhase: request.AppliesToPhase,
                    isAppliesToPhaseCustomized: true,
                    isUsedForMentoringPlan: false,
                    isMentoringPlanCustomized: true,
                    isUsedForDiagnosis: request.IsUsedForDiagnosis,
                    isDiagnosisCustomized: true,
                    order: request.Order,
                    isOrderCustomized: true);

                // Save the project
                repository.Update(project);
            }

            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created project question {QuestionId} in block {BlockId} for project {ProjectId}",
                newQuestion.Id,
                block.Id,
                project.Id);

            return Success(newQuestion.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating project question for project {ProjectId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateQuestion", "Error al crear la pregunta"));
        }
    }
}