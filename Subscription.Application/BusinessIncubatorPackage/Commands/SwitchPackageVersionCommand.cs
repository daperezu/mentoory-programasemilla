using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;

public record SwitchPackageVersionCommand(long BusinessIncubatorId, long PackageVersionId) : IBaseRequest;

public class SwitchPackageVersionCommandValidator : AbstractValidator<SwitchPackageVersionCommand>
{
    public SwitchPackageVersionCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);
        RuleFor(x => x.PackageVersionId).GreaterThan(0);
    }
}

public partial class SwitchPackageVersionCommandHandler(ILogger<SwitchPackageVersionCommandHandler> logger, IBusinessIncubatorPackageRepository incubatorRepo, IPackageRepository packageRepo, IAuditContext auditContext)
    : BaseCommandHandler<SwitchPackageVersionCommand>
{
    public override async Task<Result> Handle(SwitchPackageVersionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var incubatorPackage = await incubatorRepo.GetByIncubatorIdAsync(request.BusinessIncubatorId, cancellationToken);
            if (incubatorPackage is null)
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageNotFound);
            }

            var exists = await packageRepo.VersionExistsAsync(request.PackageVersionId, cancellationToken);
            if (!exists)
            {
                return Failure(ResultErrorCodes.Subscription_PackageVersionNotFound);
            }

            incubatorPackage.SwitchPackageVersion(request.PackageVersionId, auditContext);

            return Success();
        }
        catch (Exception ex)
        {
            LogSwitchFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorSwitchVersionFailed, (nameof(SwitchPackageVersionCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2204, Level = LogLevel.Error, Message = nameof(SwitchPackageVersionCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogSwitchFailed(string errorMessage);
}
