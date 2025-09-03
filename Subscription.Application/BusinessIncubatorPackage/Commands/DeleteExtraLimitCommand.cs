using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;

public record DeleteExtraLimitCommand(long BusinessIncubatorId, int Type, int Quantity) : IBaseRequest;

public class DeleteExtraLimitCommandValidator : AbstractValidator<DeleteExtraLimitCommand>
{
    public DeleteExtraLimitCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Type).Must(t => Enum.IsDefined(typeof(PackageLimitType), t))
            .WithMessage("Invalid limit type.");
    }
}

public partial class DeleteExtraLimitCommandHandler(ILogger<DeleteExtraLimitCommandHandler> logger, IBusinessIncubatorPackageRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<DeleteExtraLimitCommand>
{
    public override async Task<Result> Handle(DeleteExtraLimitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await repository.GetWithLimitOverridesByIncubatorIdAsync(request.BusinessIncubatorId, cancellationToken);
            if (package is null)
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
            }

            var type = (PackageLimitType)request.Type;
            package.DeleteExtraLimit(type, request.Quantity, auditContext);

            return Success();
        }
        catch (Exception ex)
        {
            LogRemoveLimitFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorRemoveLimitFailed, (nameof(DeleteExtraLimitCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2202, Level = LogLevel.Error, Message = nameof(DeleteExtraLimitCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogRemoveLimitFailed(string errorMessage);
}
