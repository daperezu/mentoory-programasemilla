using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectAnswerOption;

/// <summary>
/// Handler for updating an answer option for a project question.
/// </summary>
public sealed class UpdateProjectAnswerOptionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<UpdateProjectAnswerOptionCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectAnswerOptionCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectAnswerOptionCommand request,
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

            // Find the answer option in project blocks
            var answerOption = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions)
                .SelectMany(q => q.ProjectAnswerOptions)
                .FirstOrDefault(ao => ao.Id == request.AnswerOptionId);

            if (answerOption is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.AnswerOptionId), "Opción de respuesta no encontrada"));
            }

            // Update only the fields that have changed
            // For text: only mark as customized if it changed
            if (answerOption.Text != request.Text)
            {
                answerOption.UpdateText(request.Text, isCustomized: true);
            }

            // For score: only mark as customized if it changed
            if (answerOption.Score != request.Score)
            {
                answerOption.UpdateScore(request.Score, isCustomized: true);
            }

            // For FODA: only mark as customized if it changed
            if (answerOption.Foda != request.Foda)
            {
                answerOption.UpdateFoda(request.Foda, isCustomized: true);
            }

            // For FODA explanation: only mark as customized if it changed
            if (answerOption.FodaExplanation != request.FodaExplanation)
            {
                answerOption.UpdateFodaExplanation(request.FodaExplanation, isCustomized: true);
            }

            // For ODSR: only mark as customized if it changed
            if (answerOption.Odsr != request.Odsr)
            {
                answerOption.UpdateOdsr(request.Odsr, isCustomized: true);
            }

            // For ODSR explanation: only mark as customized if it changed
            if (answerOption.OdsrExplanation != request.OdsrExplanation)
            {
                answerOption.UpdateOdsrExplanation(request.OdsrExplanation, isCustomized: true);
            }

            // For order: always update but don't mark as customized since order is not synced
            if (answerOption.Order != request.Order)
            {
                answerOption.UpdateOrder(request.Order, isCustomized: false);
            }

            // For follow-up question: only mark as customized if it changed
            if (answerOption.FollowUpQuestionText != request.FollowUpQuestionText)
            {
                answerOption.UpdateFollowUpQuestionText(request.FollowUpQuestionText, isCustomized: true);
            }

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated answer option {AnswerOptionId} in project {ProjectId}",
                answerOption.Id,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating answer option {AnswerOptionId} for project {ProjectId}",
                request.AnswerOptionId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("UpdateAnswerOption", "Error al actualizar la opción de respuesta"));
        }
    }
}
