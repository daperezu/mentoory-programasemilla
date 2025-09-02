using FluentValidation;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.KnowledgeStructure.Application.Module.Commands;

public record CreateModuleCommand(string Name) : IBaseRequest<long>;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");
    }
}

public class CreateModuleCommandHandler(IModuleRepository moduleRepository)
    : BaseCommandHandler<CreateModuleCommand, long>
{
    public override async Task<Result<long>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        // Check if a module with the same name already exists
        var exists = await moduleRepository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Failure(
                ResultErrorCodes.Module_NameAlreadyExists,
                (nameof(request.Name), "Ya existe un módulo con el mismo nombre."));
        }

        // Create the module
        var module = new Domain.Aggregates.Module.Module(request.Name);
        moduleRepository.Add(module);

        // Save to get the module ID
        await moduleRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(module.Id);
    }
}
