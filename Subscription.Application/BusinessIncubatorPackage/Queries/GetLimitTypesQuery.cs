using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;

public record GetLimitTypesQuery : IBaseRequest<(int LimitType, string LimitName)[]>;

public class GetLimitTypesQueryHandler : BaseCommandHandler<GetLimitTypesQuery, (int LimitType, string LimitName)[]>
{
    public override Task<Result<(int LimitType, string LimitName)[]>> Handle(GetLimitTypesQuery request, CancellationToken cancellationToken)
    {
        var limitTypes = Enum.GetValues<PackageLimitType>()
            .Select(x => ((int)x, x.ToString()))
            .ToArray();

        return Task.FromResult(Result.Success(limitTypes));
    }
}
