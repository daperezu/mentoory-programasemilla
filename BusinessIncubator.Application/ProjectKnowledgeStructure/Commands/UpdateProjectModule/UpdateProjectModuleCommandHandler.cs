using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application;
using LinaSys.Shared.Domain.SeedWork;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.UpdateProjectModule;

/// <summary>
/// Handler for updating a project module.
/// </summary>
public sealed class UpdateProjectModuleCommandHandler(
    IBusinessIncubatorRepository repository,

    // IAuditContext auditContext,
    ILogger<UpdateProjectModuleCommandHandler> logger)
    : BaseCommandHandler<UpdateProjectModuleCommand>
{
    public override async Task<Result> Handle(
        UpdateProjectModuleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the project directly
            var project = await repository.GetProjectByExternalIdAsync(request.ProjectExternalId, cancellationToken);

            if (project is null)
            {
                return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Proyecto no encontrado"));
            }

            // Get the project knowledge structure
            var knowledgeStructure = await repository
                .GetProjectKnowledgeStructureAsync(project.Id, cancellationToken);

            if (knowledgeStructure is null)
            {
                return Failure(ResultErrorCodes.KnowledgeStructure_NotFound, (nameof(request.ProjectExternalId), "El proyecto no tiene estructura de conocimiento"));
            }

            // Find the module
            var module = knowledgeStructure.ProjectModules
                .FirstOrDefault(m => m.Id == request.ModuleId);

            if (module is null)
            {
                return Failure(ResultErrorCodes.Unknown, ("Module", "Módulo no encontrado"));
            }

            // Update the module
            if (module.Name != request.Name)
            {
                module.CustomizeName(request.Name);
            }

            if (module.Order != request.Order)
            {
                module.CustomizeOrder(request.Order);
            }

            // Update the business incubator
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated project module {ModuleId} for project {ProjectExternalId}",
                request.ModuleId,
                request.ProjectExternalId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error updating project module {ModuleId} for project {ProjectExternalId}",
                request.ModuleId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown, ("ModuleUpdate", "Error al actualizar el módulo"));
        }
    }
}
