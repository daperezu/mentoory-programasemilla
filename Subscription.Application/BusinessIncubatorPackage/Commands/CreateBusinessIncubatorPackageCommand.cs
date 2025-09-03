using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.BusinessIncubatorPackageAggregate;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;

public record CreateBusinessIncubatorPackageCommand(long BusinessIncubatorId, long PackageVersionId) : IBaseRequest<long>;

public class CreateBusinessIncubatorPackageCommandValidator : AbstractValidator<CreateBusinessIncubatorPackageCommand>
{
    public CreateBusinessIncubatorPackageCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorId).GreaterThan(0);
        RuleFor(x => x.PackageVersionId).GreaterThan(0);
    }
}

public partial class CreateBusinessIncubatorPackageCommandHandler(
    ILogger<CreateBusinessIncubatorPackageCommandHandler> logger,
    IBusinessIncubatorPackageRepository incubatorPackageRepository,
    IPackageRepository packageRepository,
    IAuditContext auditContext) : BaseCommandHandler<CreateBusinessIncubatorPackageCommand, long>
{
    public override async Task<Result<long>> Handle(CreateBusinessIncubatorPackageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await incubatorPackageRepository.HasPackageAsync(request.BusinessIncubatorId, cancellationToken))
            {
                return Failure(ResultErrorCodes.Subscription_BusinessIncubatorAlreadyHasPackage);
            }

            var versionExists = await packageRepository.VersionExistsAsync(request.PackageVersionId, cancellationToken);

            if (!versionExists)
            {
                return Failure(ResultErrorCodes.Subscription_PackageVersionNotFound);
            }

            var entity = new Domain.AggregatesModel.BusinessIncubatorPackageAggregate.BusinessIncubatorPackage(
                request.BusinessIncubatorId,
                request.PackageVersionId,
                auditContext);

            incubatorPackageRepository.Add(entity);

            //// Exception to the rule: we don't want to SaveChangesAsync in the handler
            await incubatorPackageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success(entity.Id);
        }
        catch (Exception ex)
        {
            LogCreationFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_BusinessIncubatorPackageCreateFailed, (nameof(CreateBusinessIncubatorPackageCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2101, Level = LogLevel.Error, Message = nameof(CreateBusinessIncubatorPackageCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogCreationFailed(string errorMessage);
}
