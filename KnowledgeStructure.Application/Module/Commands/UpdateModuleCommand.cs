using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public record UpdateModuleCommand(
    long ModuleId,
    string Name,
    string? Description) : IBaseRequest<Result>;

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("El ID del módulo debe ser válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdateModuleCommandHandler(IModuleRepository moduleRepository)
    : BaseCommandHandler<UpdateModuleCommand, Result>
{
    public override async Task<Result<Result>> Handle(
        UpdateModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);

        if (module is null)
        {
            return Failure(
                ResultErrorCodes.Module_NotFound,
                (nameof(request.ModuleId), "El módulo no existe."));
        }

        // Update module properties
        module.Rename(request.Name);

        // Note: Description is stored in KnowledgeStructureModule, not in Module itself
        // This would need to be updated through the KnowledgeStructure aggregate
        await moduleRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(Result.Success());
    }
}
