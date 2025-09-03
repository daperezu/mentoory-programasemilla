using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.Package.Commands;

public record UpdatePackageCommand(long Id, string Name) : IBaseRequest<long>;

public class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
{
    public UpdatePackageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public partial class UpdatePackageCommandHandler(ILogger<UpdatePackageCommandHandler> logger, IPackageRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<UpdatePackageCommand, long>
{
    public override async Task<Result<long>> Handle(UpdatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = await repository.FindByIdAsync(request.Id, cancellationToken);

        if (package is null)
        {
            return Failure(ResultErrorCodes.Subscription_PackageNotFound, (nameof(request.Id), $"The package with ID: {request.Id} was not found"));
        }

        try
        {
            package.UpdateName(request.Name, auditContext);

            return Success(package.Id);
        }
        catch (Exception ex)
        {
            LogUpdatePackageCommandFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_PackageUpdateFailed, (nameof(UpdatePackageCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = nameof(UpdatePackageCommandHandler) + "failed with message: {ErrorMessage}")]
    partial void LogUpdatePackageCommandFailed(string errorMessage);
}
