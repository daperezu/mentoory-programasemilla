using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.UpdateProjectBlock;

/// <summary>
/// Handler for updating a project block.
/// </summary>
public sealed class UpdateProjectBlockCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<UpdateProjectBlockCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectBlockCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectBlockCommand request,
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

            // Check if another block with the same name exists
            if (project.ProjectBlocks
                .Any(b => b.Id != request.BlockId &&
                          b.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe otro bloque con este nombre"));
            }

            // Update only if the name has changed
            if (block.Name != request.Name)
            {
                block.UpdateName(request.Name, isCustomized: true);
            }

            // Note: UpdateOrder method doesn't exist in domain model

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated project block {BlockId} in project {ProjectId}",
                block.Id,
                project.Id);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error updating project block {BlockId} for project {ProjectId}",
                request.BlockId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("UpdateBlock", "Error al actualizar el bloque"));
        }
    }
}