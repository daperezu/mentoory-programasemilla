using FluentValidation;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using LinaSys.Subscription.Domain.AggregatesModel.PackageAggregate;
using LinaSys.Subscription.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LinaSys.Subscription.Application.Package.Commands;

public record CreatePackageCommand(string Name, (PackageLimitType Type, int Quantity)[] Limits) : IBaseRequest<long>;

public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Limits).NotNull();
    }
}

public partial class CreatePackageCommandHandler(ILogger<CreatePackageCommandHandler> logger, IPackageRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<CreatePackageCommand, long>
{
    public override async Task<Result<long>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var package = new Domain.AggregatesModel.PackageAggregate.Package(request.Name, auditContext);

            var limits = request.Limits.Select(s => new PackageVersionLimit(s.Type, s.Quantity));

            package.AddVersion(new PackageVersion(request.Name, limits, auditContext));

            repository.Add(package);

            //// Exception to the rule: We shouldn't call SaveChangesAsync in the handler
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success(package.Id);
        }
        catch (Exception ex)
        {
            LogCreatePackageCommandFailed(ex.Message);
            return Failure(ResultErrorCodes.Subscription_PackageCreationFailed, (nameof(CreatePackageCommand), ex.Message));
        }
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = nameof(CreatePackageCommand) + "failed with message: {ErrorMessage}")]
    partial void LogCreatePackageCommandFailed(string errorMessage);
}
