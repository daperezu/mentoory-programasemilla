using FluentValidation;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;
using LinaSys.Subscription.Domain.Enums;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

public record DeleteBusinessIncubatorExtraLimitCommand(Guid ExternalId, int Type, int Quantity) : IBaseRequest;

public class DeleteBusinessIncubatorExtraLimitCommandValidator : AbstractValidator<DeleteBusinessIncubatorExtraLimitCommand>
{
    public DeleteBusinessIncubatorExtraLimitCommandValidator()
    {
        RuleFor(command => command.ExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator Id is required");

        RuleFor(command => command.Type)
            .Must(type => Enum.IsDefined(typeof(PackageLimitType), type))
            .WithMessage("Invalid limit type.");

        RuleFor(command => command.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}

public class DeleteBusinessIncubatorExtraLimitCommandHandler(IMediator mediator) : BaseCommandHandler<DeleteBusinessIncubatorExtraLimitCommand>
{
    public override async Task<Result> Handle(DeleteBusinessIncubatorExtraLimitCommand request, CancellationToken cancellationToken)
    {
        var getIdResult = await mediator.Send(new GetBusinessIncubatorIdQuery(request.ExternalId), cancellationToken);

        if (getIdResult.IsFailure)
        {
            return Failure(getIdResult.ErrorCode ?? ResultErrorCodes.Unknown, getIdResult.ErrorMessages ?? []);
        }

        var incubatorId = getIdResult.Value;

        var clearResult = await mediator.Send(new DeleteExtraLimitCommand(incubatorId, request.Type, request.Quantity), cancellationToken);

        if (clearResult.IsFailure)
        {
            return Failure(clearResult.ErrorCode ?? ResultErrorCodes.Unknown, clearResult.ErrorMessages ?? []);
        }

        return Success();
    }
}
