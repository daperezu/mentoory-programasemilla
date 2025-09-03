using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;

namespace LinaSys.Subscription.Application.Package.Queries;

public record GetPackageByIdQuery(long Id) : IBaseRequest<PackageDetailsDto?>;

public class GetPackageByIdQueryValidator : AbstractValidator<GetPackageByIdQuery>
{
    public GetPackageByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPackageByIdQueryHandler(IPackageRepository repository)
    : BaseCommandHandler<GetPackageByIdQuery, PackageDetailsDto?>
{
    public override async Task<Result<PackageDetailsDto?>> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await repository.GetByIdWithVersionsAndLimits(request.Id, cancellationToken);

        if (package is null)
        {
            return Failure(ResultErrorCodes.Subscription_PackageNotFound);
        }

        var dto = new PackageDetailsDto(
            package.Id,
            package.Name,
            package.PackageVersions
                .Select(v => new PackageVersionDto(
                    v.Id,
                    v.Label,
                    v.PackageVersionLimits
                        .Select(l => new PackageVersionLimitDto((int)l.Type, l.Quantity))
                        .ToList()))
                .ToList());

        return Success(dto);
    }
}

public record PackageDetailsDto(long Id, string Name, List<PackageVersionDto> Versions);

public record PackageVersionDto(long Id, string Label, List<PackageVersionLimitDto> Limits);

public record PackageVersionLimitDto(int Type, int Quantity);
