using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Commands;

public record UpdateBlockCommand(long Id, string Name) : IBaseRequest;

public class UpdateBlockCommandValidator : AbstractValidator<UpdateBlockCommand>
{
    public UpdateBlockCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Block ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");
    }
}

public class UpdateBlockCommandHandler(IBlockRepository repository)
    : BaseCommandHandler<UpdateBlockCommand>
{
    public override async Task<Result> Handle(UpdateBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (block is null)
        {
            return Failure(ResultErrorCodes.Block_NotFound, (nameof(request.Id), "Bloque no encontrado."));
        }

        // Check if another block with the same name exists (excluding current block)
        var existingBlock = await repository.GetByNameAsync(request.Name, cancellationToken);
        if (existingBlock is not null && existingBlock.Id != request.Id)
        {
            return Failure(ResultErrorCodes.Block_NameAlreadyExists, (nameof(request.Name), "Ya existe otro bloque con ese nombre."));
        }

        block.Rename(request.Name);

        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
