using LinaSys.BusinessIncubator.Domain.Aggregates.BusinessIncubator;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectQuestion;

/// <summary>
/// Handler for deleting a project question.
/// </summary>
public sealed class DeleteProjectQuestionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectQuestionCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectQuestionCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectQuestionCommand request,
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
            ProjectBlock? parentBlock = null;
            var question = project.ProjectBlocks
                .SelectMany(b =>
                {
                    var q = b.ProjectQuestions.FirstOrDefault(q => q.Id == request.QuestionId);
                    if (q is not null)
                    {
                        parentBlock = b;
                    }

                    return b.ProjectQuestions;
                })
                .FirstOrDefault(q => q.Id == request.QuestionId);

            if (question is null || parentBlock is null)
            {
                return Failure(ResultErrorCodes.Question_NotFound,
                    (nameof(request.QuestionId), "Pregunta no encontrada"));
            }

            // Check if question has answer options
            if (question.ProjectAnswerOptions.Any())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Question", "No se puede eliminar una pregunta que contiene opciones de respuesta"));
            }

            // Remove the question
            parentBlock.RemoveProjectQuestion(request.QuestionId);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted project question {QuestionId} from project {ProjectId}",
                request.QuestionId,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting project question {QuestionId} for project {ProjectId}",
                request.QuestionId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("DeleteQuestion", "Error al eliminar la pregunta"));
        }
    }
}