using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;

public record AddExtraLimitCommand(long BusinessIncubatorId, int Type, int Quantity) : IBaseRequest;

public class AddExtraLimitCommandValidator : AbstractValidator<AddExtraLimitCommand>
{
    public AddExtraLimitCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);

        RuleFor(x => x.Quantity).GreaterThan(0);

        RuleFor(x => x.Type).Must(type => Enum.IsDefined(typeof(PackageLimitType), type))
            .WithMessage("Invalid limit type.");
    }
}

public partial class AddExtraLimitCommandHandler(ILogger<AddExtraLimitCommandHandler> logger, IBusinessIncubatorPackageRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<AddExtraLimitCommand>
{
    public override async Task<Result> Handle(AddExtraLimitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await repository.GetByIncubatorIdAsync(request.BusinessIncubatorId, cancellationToken);
            if (package is null)
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
            }

            var type = (PackageLimitType)request.Type;
            package.AddExtraLimit(type, request.Quantity, auditContext);

            return Success();
        }
        catch (Exception ex)
        {
            LogAddLimitFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorAddLimitFailed, (nameof(AddExtraLimitCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2201, Level = LogLevel.Error, Message = nameof(AddExtraLimitCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogAddLimitFailed(string errorMessage);
}
