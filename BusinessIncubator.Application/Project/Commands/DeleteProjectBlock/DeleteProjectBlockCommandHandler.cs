using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.DeleteProjectBlock;

/// <summary>
/// Handler for deleting a project block.
/// </summary>
public sealed class DeleteProjectBlockCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectBlockCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectBlockCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectBlockCommand request,
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

            // Find the block
            var block = project.ProjectBlocks.FirstOrDefault(b => b.Id == request.BlockId);

            if (block is null)
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.BlockId), "Bloque no encontrado"));
            }

            // Check if block has questions
            if (block.ProjectQuestions.Any())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Block", "No se puede eliminar un bloque que contiene preguntas"));
            }

            // Remove the block
            project.RemoveBlock(block);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted project block {BlockId} from project {ProjectId}",
                request.BlockId,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error deleting project block {BlockId} for project {ProjectId}",
                request.BlockId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("DeleteBlock", "Error al eliminar el bloque"));
        }
    }
}
