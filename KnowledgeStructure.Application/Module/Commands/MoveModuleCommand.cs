using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public sealed record MoveModuleCommand(long ModuleId, long KnowledgeStructureId, int NewPosition) : IBaseRequest;

public sealed class MoveModuleCommandHandler(IKnowledgeStructureRepository repository)
    : BaseCommandHandler<MoveModuleCommand>
{
    public override async Task<Result> Handle(MoveModuleCommand request, CancellationToken cancellationToken)
    {
        var structure = await repository.GetWithModulesAsync(request.KnowledgeStructureId, cancellationToken);
        if (structure is null)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NotFound,
                ("KnowledgeStructure", $"Estructura de conocimiento con ID {request.KnowledgeStructureId} no encontrada"));
        }

        var module = structure.KnowledgeStructureModules.FirstOrDefault(m => m.Id == request.ModuleId);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                ("Module", $"Módulo con ID {request.ModuleId} no encontrado"));
        }

        // Reorder modules
        var modules = structure.KnowledgeStructureModules.OrderBy(m => m.Order).ToList();
        modules.Remove(module);

        // Ensure the position is within bounds
        var insertPosition = Math.Max(0, Math.Min(request.NewPosition, modules.Count));
        modules.Insert(insertPosition, module);

        // Update order for all modules
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Reorder(i + 1);
        }

        await repository.UpdateKnowledgeStructureAsync(structure, cancellationToken);
        return Success();
    }
}
