using FluentValidation;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public sealed record GetFormBlocksByIdQuery(long FormId) : IBaseRequest<List<BlockDto>>;

public sealed record BlockDto(long Id, string Name);

public class GetFormBlocksByIdQueryValidator : AbstractValidator<GetFormBlocksByIdQuery>
{
    public GetFormBlocksByIdQueryValidator()
    {
        RuleFor(query => query.FormId)
            .GreaterThan(0)
            .WithMessage("Form ID must be greater than 0.");
    }
}

public class GetFormBlocksByIdQueryHandler(IFormRepository formRepository, IBlockRepository blockRepository)
    : BaseCommandHandler<GetFormBlocksByIdQuery, List<BlockDto>>
{
    public override async Task<Result<List<BlockDto>>> Handle(GetFormBlocksByIdQuery request, CancellationToken cancellationToken)
    {
        var blockIds = await formRepository.GetBlockIdsByFormIdAsync(request.FormId, cancellationToken).ConfigureAwait(false);

        if (!blockIds.Any())
        {
            return Failure(ResultErrorCodes.DiagnosisForm_Blocks_NotFound, (nameof(request.FormId), "No blocks found for the given form ID."));
        }

        var blocks = await blockRepository.GetBlocksByIdsAsync(blockIds, cancellationToken).ConfigureAwait(false);

        if (!blocks.Any())
        {
            return Failure(ResultErrorCodes.DiagnosisForm_Blocks_NotFound, (nameof(request.FormId), "No blocks found for the given form ID."));
        }

        var blockDtos = blocks.Select(block => new BlockDto(block.Id, block.Name)).ToList();
        return Success(blockDtos);
    }
}
