using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Diagnostics.Application.Form.Queries;

public sealed record GetAllQuestionPhasesQuery() : IBaseRequest<Dictionary<int, string>>;

public sealed class GetQuestionPhasesQueryHandler
    : BaseCommandHandler<GetAllQuestionPhasesQuery, Dictionary<int, string>>
{
    public override Task<Result<Dictionary<int, string>>> Handle(GetAllQuestionPhasesQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Success(EnumHelper.ToDictionary<QuestionPhase>()));
}
