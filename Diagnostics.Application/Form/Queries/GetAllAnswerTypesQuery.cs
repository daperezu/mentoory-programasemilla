using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public sealed record GetAllAnswerTypesQuery() : IBaseRequest<Dictionary<int, string>>;

public sealed class GetAnswerTypesQueryHandler
    : BaseCommandHandler<GetAllAnswerTypesQuery, Dictionary<int, string>>
{
    public override Task<Result<Dictionary<int, string>>> Handle(GetAllAnswerTypesQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Success(EnumHelper.ToDictionary<AnswerType>()));
}
