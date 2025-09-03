using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Commands;

public record DeleteBlockCommand(long Id) : IBaseRequest;

public class DeleteBlockCommandValidator : AbstractValidator<DeleteBlockCommand>
{
    public DeleteBlockCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Block ID must be greater than 0.");
    }
}

public class DeleteBlockCommandHandler(IBlockRepository repository)
    : BaseCommandHandler<DeleteBlockCommand>
{
    public override async Task<Result> Handle(DeleteBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (block is null)
        {
            return Failure(ResultErrorCodes.Block_NotFound, (nameof(request.Id), "Bloque no encontrado."));
        }

        // Check if block is being used in any forms/questions
        var isInUse = await repository.IsBlockInUseAsync(request.Id, cancellationToken);
        if (isInUse)
        {
            return Failure(
                ResultErrorCodes.Block_CannotDeleteUsedBlock,
                (nameof(request.Id), "No se puede eliminar un bloque que está siendo utilizado en preguntas."));
        }

        repository.Remove(block);

        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
