using FluentValidation;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Application.BusinessIncubatorPackage.Queries;
using MediatR;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Queries;

public record GetBusinessIncubatorDetailsWithSubscriptionPackageQuery(Guid Id) : IBaseRequest<BusinessIncubatorWithPackageDto>;

public record BusinessIncubatorWithPackageDto(BusinessIncubatorDetailsDto BusinessIncubatorDetails, BusinessIncubatorPackageAndLimitsDto PackageAndLimits);

public class GetBusinessIncubatorDetailsWithSubscriptionPackageQueryValidator : AbstractValidator<GetBusinessIncubatorDetailsWithSubscriptionPackageQuery>
{
    public GetBusinessIncubatorDetailsWithSubscriptionPackageQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetBusinessIncubatorDetailsWithSubscriptionPackageQueryHandler(IMediator mediator)
   : BaseCommandHandler<GetBusinessIncubatorDetailsWithSubscriptionPackageQuery, BusinessIncubatorWithPackageDto>
{
    public override async Task<Result<BusinessIncubatorWithPackageDto>> Handle(GetBusinessIncubatorDetailsWithSubscriptionPackageQuery request, CancellationToken cancellationToken)
    {
        var incubatorResult = await mediator.Send(new GetBusinessIncubatorDetailsQuery(request.Id), cancellationToken);

        if (incubatorResult.IsFailure || incubatorResult.Value is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.Id), "Business Incubator not found."));
        }

        var packageResult = await mediator.Send(new GetBusinessIncubatorPackageAndLimitsQuery(incubatorResult.Value.Id), cancellationToken);

        if (packageResult.IsFailure || packageResult.Value is null)
        {
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound, (nameof(request.Id), "Business Incubator Package not found."));
        }

        var dto = new BusinessIncubatorWithPackageDto(incubatorResult.Value, packageResult.Value);

        return Success(dto);
    }
}
