using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.Enums;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;

public record GetBusinessIncubatorPackageAndLimitsQuery(long Id) : IBaseRequest<BusinessIncubatorPackageAndLimitsDto>;

public class GetBusinessIncubatorPackageAndLimitsQueryValidator : AbstractValidator<GetBusinessIncubatorPackageAndLimitsQuery>
{
    public GetBusinessIncubatorPackageAndLimitsQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Business Incubator Id is required.")
            .GreaterThan(0)
            .WithMessage("Business Incubator Id must be greater than 0.");
    }
}

public record BusinessIncubatorPackageAndLimitsDto(string PackageLabel, long PackageVersionId, string PackageVersionLabel, List<PackageLimit> PackageLimits, List<PackageLimit> LimitOverrides, List<PackageLimit> EffectiveLimits);

public record PackageLimit(int Type, int Quantity);

public partial class GetBusinessIncubatorPackageAndLimitsQueryHandler(IBusinessIncubatorPackageRepository repository)
    : BaseCommandHandler<GetBusinessIncubatorPackageAndLimitsQuery, BusinessIncubatorPackageAndLimitsDto>
{
    public override async Task<Result<BusinessIncubatorPackageAndLimitsDto>> Handle(GetBusinessIncubatorPackageAndLimitsQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetWithVersionAndLimitsByIncubatorIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
        }

        var packageVersionLimits = entity.PackageVersion.PackageVersionLimits
            .Select(x => new PackageLimit((int)x.Type, x.Quantity))
            .ToList();

        var limitOverrides = entity.PackageLimitOverrides
            .Select(x => new PackageLimit((int)x.Type, x.Quantity))
            .ToList();

        var limitTypes = packageVersionLimits.Select(s => s.Type).Distinct();
        var effectiveLimits = limitTypes.Select(limitType => new PackageLimit(limitType, entity.GetEffectiveLimit((PackageLimitType)limitType))).ToList();

        var dto = new BusinessIncubatorPackageAndLimitsDto(
            entity.PackageVersion.Package.Name,
            entity.PackageVersion.Id,
            entity.PackageVersion.Label,
            packageVersionLimits,
            limitOverrides,
            effectiveLimits);

        return Result.Success(dto);
    }
}
