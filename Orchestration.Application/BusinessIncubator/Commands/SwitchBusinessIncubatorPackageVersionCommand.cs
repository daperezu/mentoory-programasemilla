using FluentValidation;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

public record SwitchBusinessIncubatorPackageVersionCommand(Guid Id, long PackageVersionId) : IBaseRequest;

public class SwitchBusinessIncubatorPackageVersionCommandValidator : AbstractValidator<SwitchBusinessIncubatorPackageVersionCommand>
{
    public SwitchBusinessIncubatorPackageVersionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.PackageVersionId).GreaterThan(0);
    }
}

public class SwitchBusinessIncubatorPackageVersionCommandHandler(IMediator mediator)
    : BaseCommandHandler<SwitchBusinessIncubatorPackageVersionCommand>
{
    public override async Task<Result> Handle(SwitchBusinessIncubatorPackageVersionCommand request, CancellationToken cancellationToken)
    {
        var getIdResult = await mediator.Send(new GetBusinessIncubatorIdQuery(request.Id), cancellationToken);

        if (getIdResult.IsFailure)
        {
            return Failure(getIdResult.ErrorCode ?? ResultErrorCodes.Unknown, getIdResult.ErrorMessages ?? []);
        }

        var incubatorId = getIdResult.Value;

        var switchResult = await mediator.Send(new SwitchPackageVersionCommand(incubatorId, request.PackageVersionId), cancellationToken);

        if (switchResult.IsFailure)
        {
            return Failure(switchResult.ErrorCode ?? ResultErrorCodes.Unknown, switchResult.ErrorMessages ?? []);
        }

        return Success();
    }
}
