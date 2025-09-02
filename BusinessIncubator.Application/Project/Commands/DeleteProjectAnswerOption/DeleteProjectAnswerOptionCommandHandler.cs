using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectAnswerOption;

/// <summary>
/// Handler for deleting an answer option from a project question.
/// </summary>
public sealed class DeleteProjectAnswerOptionCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectAnswerOptionCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectAnswerOptionCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectAnswerOptionCommand request,
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

            // Find the question that contains the answer option in project blocks
            var question = project.ProjectBlocks
                .SelectMany(b => b.ProjectQuestions)
                .FirstOrDefault(q => q.ProjectAnswerOptions.Any(ao => ao.Id == request.AnswerOptionId));

            if (question is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.AnswerOptionId), "Opción de respuesta no encontrada"));
            }

            // Remove the answer option
            question.RemoveProjectAnswerOption(request.AnswerOptionId);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted answer option {AnswerOptionId} from project {ProjectId}",
                request.AnswerOptionId,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting answer option {AnswerOptionId} from project {ProjectId}",
                request.AnswerOptionId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("DeleteAnswerOption", "Error al eliminar la opción de respuesta"));
        }
    }
}