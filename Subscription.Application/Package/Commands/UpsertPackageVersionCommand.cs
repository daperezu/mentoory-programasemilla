using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using LinaSys.Subscription.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.Package.Commands;

public record UpsertPackageVersionCommand(
    long PackageId,
    long VersionId,
    string Label,
    List<(PackageLimitType Type, int Quantity)> Limits) : IBaseRequest;

public class UpsertPackageVersionCommandValidator : AbstractValidator<UpsertPackageVersionCommand>
{
    public UpsertPackageVersionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PackageId).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Limits).NotEmpty();
    }
}

public partial class UpsertPackageVersionCommandHandler(
    ILogger<UpsertPackageVersionCommandHandler> logger,
    IPackageRepository repository,
    IAuditContext auditContext) : BaseCommandHandler<UpsertPackageVersionCommand>
{
    public override async Task<Result> Handle(UpsertPackageVersionCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.FindByIdAsync(request.PackageId, cancellationToken);

        if (package is null)
        {
            return Failure(ResultErrorCodes.Subscription_PackageNotFound, (nameof(request.PackageId), $"Package {request.PackageId} not found"));
        }

        try
        {
            var limits = request.Limits
                .Select(l => new PackageVersionLimit(l.Type, l.Quantity))
                .ToList();

            var version = new PackageVersion(request.VersionId, request.Label, limits, auditContext);

            package.UpsertVersion(version);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            LogUpsertVersionFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_PackageVersionNotFound, (nameof(UpsertPackageVersionCommand), ex.Message));
        }
        catch (Exception ex)
        {
            LogUpsertVersionFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_PackageVersionUpsertFailed, (nameof(UpsertPackageVersionCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Error, Message = nameof(UpsertPackageVersionCommandHandler) + " failed: {ErrorMessage}")]
    partial void LogUpsertVersionFailed(string errorMessage);
}
