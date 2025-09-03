using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public record DeleteModuleCommand(long ModuleId) : IBaseRequest<Result>;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo debe ser válido.");
    }
}

public class DeleteModuleCommandHandler(
    IModuleRepository moduleRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<DeleteModuleCommand, Result>
{
    public override async Task<Result<Result>> Handle(
        DeleteModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);

        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no existe."));
        }

        // Find all knowledge structures that contain this module
        var allStructures = await knowledgeStructureRepository.GetAllWithModulesAsync(cancellationToken);

        foreach (var structure in allStructures)
        {
            var moduleToRemove = structure.KnowledgeStructureModules
                .FirstOrDefault(ksm => ksm.ModuleId == request.ModuleId);

            if (moduleToRemove is not null)
            {
                structure.RemoveModule(moduleToRemove.Id);
            }
        }

        // Note: We're not deleting the module itself, just removing it from structures
        // If you want to delete the module completely, add:
        // moduleRepository.Remove(module);
        await knowledgeStructureRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(Result.Success());
    }
}
