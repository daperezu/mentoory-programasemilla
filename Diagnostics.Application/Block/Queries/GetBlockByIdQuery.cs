using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Block.Queries;

public sealed record GetBlockByIdQuery(long Id) : IBaseRequest<BlockDto>;

public sealed record BlockDto(long Id, string Name);

public class GetBlockByIdQueryValidator : AbstractValidator<GetBlockByIdQuery>
{
    public GetBlockByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Block ID must be greater than 0.");
    }
}

public sealed class GetBlockByIdQueryHandler(IBlockRepository repository)
    : BaseCommandHandler<GetBlockByIdQuery, BlockDto>
{
    public override async Task<Result<BlockDto>> Handle(GetBlockByIdQuery request, CancellationToken cancellationToken)
    {
        var block = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (block is null)
        {
            return Failure(ResultErrorCodes.Block_NotFound, (nameof(request.Id), "Bloque no encontrado."));
        }

        var dto = new BlockDto(block.Id, block.Name);
        return Success(dto);
    }
}
