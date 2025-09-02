using FluentValidation;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;
using MediatR;
using IBaseRequest = LinaSys.Shared.Application.MediatR.IBaseRequest;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to clear all extra limits for a Business Incubator.
/// </summary>
/// <param name="ExternalId">The unique identifier of the Business Incubator.</param>
public record ClearBusinessIncubatorExtraLimitsCommand(Guid ExternalId) : IBaseRequest;

/// <summary>
/// Validator for <see cref="ClearBusinessIncubatorExtraLimitsCommand"/>.
/// </summary>
public class ClearBusinessIncubatorExtraLimitsCommandValidator : AbstractValidator<ClearBusinessIncubatorExtraLimitsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearBusinessIncubatorExtraLimitsCommandValidator"/> class.
    /// </summary>
    public ClearBusinessIncubatorExtraLimitsCommandValidator()
    {
        RuleFor(command => command.ExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator Id is required");
    }
}

/// <summary>
/// Handler for <see cref="ClearBusinessIncubatorExtraLimitsCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for clearing all extra limits associated with a Business Incubator.
/// It validates the existence of the Business Incubator before attempting to clear the limits.
/// </remarks>
public class ClearBusinessIncubatorExtraLimitsCommandHanlder(IMediator mediator) : BaseCommandHandler<ClearBusinessIncubatorExtraLimitsCommand>
{
    /// <summary>
    /// Handles the clearing of all extra limits for a Business Incubator.
    /// </summary>
    /// <param name="request">The command request containing the Business Incubator ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Verifies the existence of the Business Incubator by its ID.
    /// - If the Business Incubator exists, proceeds to clear all its extra limits.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or if limit clearing fails.
    /// </summary>
    public override async Task<Result> Handle(ClearBusinessIncubatorExtraLimitsCommand request, CancellationToken cancellationToken)
    {
        var getIdResult = await mediator.Send(new GetBusinessIncubatorIdQuery(request.ExternalId), cancellationToken);

        if (getIdResult.IsFailure)
        {
            return Failure(getIdResult.ErrorCode ?? ResultErrorCodes.Unknown, getIdResult.ErrorMessages ?? []);
        }

        var incubatorId = getIdResult.Value;

        var clearResult = await mediator.Send(new ClearExtraLimitsCommand(incubatorId), cancellationToken);

        if (clearResult.IsFailure)
        {
            return Failure(clearResult.ErrorCode ?? ResultErrorCodes.Unknown, clearResult.ErrorMessages ?? []);
        }

        return Success();
    }
}
