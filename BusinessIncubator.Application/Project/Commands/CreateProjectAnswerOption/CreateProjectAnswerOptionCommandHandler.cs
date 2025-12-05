using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectAnswerOption;

/// <summary>
/// Handler for creating a new answer option for a project question.
/// </summary>
public sealed class CreateProjectAnswerOptionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<CreateProjectAnswerOptionCommandHandler> logger)
    : BaseCommandHandler<CreateProjectAnswerOptionCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectAnswerOptionCommand request,
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

            // Find the question in project blocks
            var question = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions)
                .FirstOrDefault(q => q.Id == request.QuestionId);

            if (question is null)
            {
                return Failure(ResultErrorCodes.Question_NotFound,
                    (nameof(request.QuestionId), "Pregunta no encontrada"));
            }

            // Add the answer option
            var answerOption = question.AddProjectAnswerOption(
                sourceAnswerOptionId: null,
                text: request.Text,
                isTextCustomized: true,
                score: request.Score,
                isScoreCustomized: true,
                foda: request.Foda,
                isFodaCustomized: true,
                fodaExplanation: request.FodaExplanation,
                isFodaExplanationCustomized: true,
                odsr: request.Odsr,
                isOdsrCustomized: true,
                odsrExplanation: request.OdsrExplanation,
                isOdsrExplanationCustomized: true,
                order: request.Order,
                isOrderCustomized: true,
                followUpQuestionText: request.FollowUpQuestionText ?? string.Empty,
                isFollowUpTextCustomized: true);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created answer option {AnswerOptionId} for question {QuestionId} in project {ProjectId}",
                answerOption.Id,
                question.Id,
                project.Id);

            return Success(answerOption.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating answer option for question {QuestionId} in project {ProjectId}",
                request.QuestionId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateAnswerOption", "Error al crear la opción de respuesta"));
        }
    }
}
