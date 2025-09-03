using Microsoft.Extensions.Logging;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Application;
using LinaSys.Shared.Domain.SeedWork;
namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.DeleteProjectModule;

/// <summary>
/// Handler for deleting a project module.
/// </summary>
public sealed class DeleteProjectModuleCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<DeleteProjectModuleCommandHandler> logger)
    : BaseCommandHandler<DeleteProjectModuleCommand>
{
    public override async Task<Result> Handle(
        DeleteProjectModuleCommand request,
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

            // Check if module has topics
            if (module.ProjectTopics.Any())
            {
                return Failure(ResultErrorCodes.Unknown,
                    ("Module", "No se puede eliminar un módulo que contiene temas"));
            }

            // Remove the module
            knowledgeStructure.RemoveProjectModule(module.Id);

            // Update the project
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted project module {ModuleId} from project {ProjectExternalId}",
                request.ModuleId,
                request.ProjectExternalId);

            return Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error deleting project module {ModuleId} from project {ProjectExternalId}",
                request.ModuleId,
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown, ("ModuleDelete", "Error al eliminar el módulo"));
        }
    }
}
