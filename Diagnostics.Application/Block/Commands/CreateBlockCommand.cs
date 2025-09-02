using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Commands;

public record CreateBlockCommand(string Name) : IBaseRequest<long>;

public class CreateBlockCommandValidator : AbstractValidator<CreateBlockCommand>
{
    public CreateBlockCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");
    }
}

public class CreateBlockCommandHandler(IBlockRepository repository)
    : BaseCommandHandler<CreateBlockCommand, long>
{
    public override async Task<Result<long>> Handle(CreateBlockCommand request, CancellationToken cancellationToken)
    {
        if (await repository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            return Failure(ResultErrorCodes.Block_NameAlreadyExists, (nameof(request.Name), "Another block with that name already exists."));
        }

        var block = new Domain.Aggregates.Block.Block(request.Name);
        repository.Add(block);

        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(block.Id);
    }
}
