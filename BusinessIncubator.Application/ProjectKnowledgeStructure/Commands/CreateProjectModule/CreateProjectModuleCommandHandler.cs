using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using Microsoft.Extensions.Logging;

namespace LinaSys.BusinessIncubator.Application.ProjectKnowledgeStructure.Commands.CreateProjectModule;

/// <summary>
/// Handler for creating a new project module.
/// </summary>
public sealed class CreateProjectModuleCommandHandler(
    IBusinessIncubatorRepository repository,
    ILogger<CreateProjectModuleCommandHandler> logger)
    : BaseCommandHandler<CreateProjectModuleCommand, long>
{
    public override async Task<Result<long>> Handle(
        CreateProjectModuleCommand request,
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

            // Check if a module with the same name already exists
            if (knowledgeStructure.ProjectModules
                .Any(m => m.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(ResultErrorCodes.Unknown,
                    (nameof(request.Name), "Ya existe un módulo con este nombre"));
            }

            // Create the new module
            var newModule = knowledgeStructure.AddProjectModule(
                sourceModuleId: null,
                name: request.Name,
                isNameCustomized: true,
                order: request.Order,
                isOrderCustomized: true);

            // Save changes
            repository.Update(project);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created project module {ModuleId} for project {ProjectId}",
                newModule.Id,
                project.Id);

            return Success(newModule.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating project module for project {ProjectId}",
                request.ProjectExternalId);

            return Failure(ResultErrorCodes.Unknown,
                ("CreateModule", "Error al crear el módulo"));
        }
    }
}
