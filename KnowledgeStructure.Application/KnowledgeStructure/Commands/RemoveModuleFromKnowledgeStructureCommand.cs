using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;

public sealed record RemoveModuleFromKnowledgeStructureCommand(
    long KnowledgeStructureId,
    long StructureModuleId) : IBaseRequest;

public sealed class RemoveModuleFromKnowledgeStructureCommandValidator : AbstractValidator<RemoveModuleFromKnowledgeStructureCommand>
{
    public RemoveModuleFromKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.KnowledgeStructureId)
            .GreaterThan(0).WithMessage("El ID de la estructura de conocimiento debe ser mayor a 0.");

        RuleFor(x => x.StructureModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo de estructura debe ser mayor a 0.");
    }
}

public sealed class RemoveModuleFromKnowledgeStructureCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository)
    : BaseCommandHandler<RemoveModuleFromKnowledgeStructureCommand>
{
    public override async Task<Result> Handle(
        RemoveModuleFromKnowledgeStructureCommand request,
        CancellationToken cancellationToken)
    {
        // Get the knowledge structure with its modules
        var knowledgeStructure = await knowledgeStructureRepository.GetWithModulesAsync(
            request.KnowledgeStructureId,
            cancellationToken);

        if (knowledgeStructure is null)
        {
            return Failure(
                ResultErrorCodes.KnowledgeStructure_NotFound,
                (nameof(request.KnowledgeStructureId), "La estructura de conocimiento no existe."));
        }

        // Find the structure module
        var structureModule = knowledgeStructure.KnowledgeStructureModules
            .FirstOrDefault(m => m.Id == request.StructureModuleId);

        if (structureModule is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.StructureModuleId), "El módulo no está asignado a esta estructura de conocimiento."));
        }

        // Check if the module has topics - prevent deletion if it has content
        if (structureModule.KnowledgeStructureTopics.Any())
        {
            return Failure(
                ResultErrorCodes.Module_HasDependencies,
                (nameof(request.StructureModuleId), "No se puede eliminar el módulo porque tiene temas asociados."));
        }

        // Remove the module from the knowledge structure
        knowledgeStructure.RemoveModule(request.StructureModuleId);

        // Save changes
        await knowledgeStructureRepository.UpdateKnowledgeStructureAsync(knowledgeStructure, cancellationToken);

        return Success();
    }
}
