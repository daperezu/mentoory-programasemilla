using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.KnowledgeStructure.Commands;

public sealed record AddModuleToKnowledgeStructureCommand(
    long KnowledgeStructureId,
    long ModuleId,
    int? Order = null) : IBaseRequest;

public sealed class AddModuleToKnowledgeStructureCommandValidator : AbstractValidator<AddModuleToKnowledgeStructureCommand>
{
    public AddModuleToKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.KnowledgeStructureId)
            .GreaterThan(0).WithMessage("El ID de la estructura de conocimiento debe ser mayor a 0.");

        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo debe ser mayor a 0.");

        RuleFor(x => x.Order)
            .GreaterThan(0).When(x => x.Order.HasValue)
            .WithMessage("El orden debe ser mayor a 0.");
    }
}

public sealed class AddModuleToKnowledgeStructureCommandHandler(
    IKnowledgeStructureRepository knowledgeStructureRepository,
    IModuleRepository moduleRepository)
    : BaseCommandHandler<AddModuleToKnowledgeStructureCommand>
{
    public override async Task<Result> Handle(
        AddModuleToKnowledgeStructureCommand request,
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

        // Get the module
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no existe."));
        }

        // Check if module is already in this knowledge structure
        var existingModule = knowledgeStructure.KnowledgeStructureModules
            .FirstOrDefault(m => m.ModuleId == request.ModuleId);

        if (existingModule is not null)
        {
            return Failure(
                ResultErrorCodes.Module_AlreadyExists,
                (nameof(request.ModuleId), "El módulo ya está asignado a esta estructura de conocimiento."));
        }

        // Determine the order
        var order = request.Order ?? (knowledgeStructure.KnowledgeStructureModules.Count + 1);

        // Add the module to the knowledge structure
        knowledgeStructure.AddModule(module, order);

        // Save changes
        await knowledgeStructureRepository.UpdateKnowledgeStructureAsync(knowledgeStructure, cancellationToken);

        return Success();
    }
}
