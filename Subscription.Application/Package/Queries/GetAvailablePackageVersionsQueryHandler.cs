using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

namespace LinaSys.Subscription.Application.Package.Queries;

public record GetAvailablePackageVersionsQuery : IBaseRequest<List<AvailablePackageVersionDto>>;

public record AvailablePackageVersionDto(long Id, string Label);

public class GetAvailablePackageVersionsQueryHandler(IPackageRepository repo)
    : BaseCommandHandler<GetAvailablePackageVersionsQuery, List<AvailablePackageVersionDto>>
{
    public override async Task<Result<List<AvailablePackageVersionDto>>> Handle(GetAvailablePackageVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await repo.GetAvailableVersionsAsync(cancellationToken); // You implement this
        var dto = versions.Select(v => new AvailablePackageVersionDto(v.Id, v.Label)).ToList();
        return Success(dto);
    }
}
