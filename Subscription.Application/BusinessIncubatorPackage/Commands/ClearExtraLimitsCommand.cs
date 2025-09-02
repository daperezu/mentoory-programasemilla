using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;

public record ClearExtraLimitsCommand(long BusinessIncubatorId) : IBaseRequest;

public class ClearExtraLimitsCommandValidator : AbstractValidator<ClearExtraLimitsCommand>
{
    public ClearExtraLimitsCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);
    }
}

public partial class ClearExtraLimitsCommandHandler(ILogger<ClearExtraLimitsCommandHandler> logger, IBusinessIncubatorPackageRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<ClearExtraLimitsCommand>
{
    public override async Task<Result> Handle(ClearExtraLimitsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await repository.GetWithLimitOverridesByIncubatorIdAsync(request.BusinessIncubatorId, cancellationToken);
            if (package is null)
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
            }

            package.ClearExtraLimits(auditContext);

            return Success();
        }
        catch (Exception ex)
        {
            LogClearFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorClearLimitsFailed, (nameof(ClearExtraLimitsCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2203, Level = LogLevel.Error, Message = nameof(ClearExtraLimitsCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogClearFailed(string errorMessage);
}
