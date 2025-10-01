using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.CreateProjectBlock;

/// <summary>
/// Handler for creating a new project block.
/// </summary>
public sealed class CreateProjectBlockCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext,
    ILogger<CreateProjectBlockCommandHandler> logger)
    : BaseCommandHandler<CreateProjectBlockCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectBlockCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project
            var project = await repository.GetProjectWithBlocksByExternalIdAsync(
                request.ProjectExternalId,
                cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound,
                    (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Check if a block with the same name already exists
            if (project.ProjectBlocks.Any(b => b.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Project_CreationFailed,
                    (nameof(request.Name), "Ya existe un bloque con este nombre"));
            }

            // Create the new block
            var newBlock = project.AddBlock(
                name: request.Name,
                sourceBlockId: null,
                auditContext: auditContext);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created project block {BlockId} for project {ProjectId}",
                newBlock.Id,
                project.Id);

            return Success(newBlock.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating project block for project {ProjectId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateBlock", "Error al crear el bloque"));
        }
    }
}
