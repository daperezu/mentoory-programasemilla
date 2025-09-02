using FluentValidation;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Subscription.Application.BusinessIncubatorPackage.Commands;
using MediatR;

namespace LinaSys.Orchestration.Application.BusinessIncubator.Commands;

public record CreateBusinessIncubatorWithPackageCommand(
    string Name,
    string? Description,
    string Key,
    long PackageVersionId) : IBaseRequest<Guid>;

public class CreateBusinessIncubatorWithPackageCommandValidator : AbstractValidator<CreateBusinessIncubatorWithPackageCommand>
{
    public CreateBusinessIncubatorWithPackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.Key).NotEmpty().MaximumLength(50);

        RuleFor(x => x.PackageVersionId).GreaterThan(0);
    }
}

public class CreateBusinessIncubatorWithPackageCommandHandler(IMediator mediator)
    : BaseCommandHandler<CreateBusinessIncubatorWithPackageCommand, Guid>
{
    public override async Task<Result<Guid>> Handle(CreateBusinessIncubatorWithPackageCommand request, CancellationToken cancellationToken)
    {
        var createResult = await mediator.Send(
            new CreateBusinessIncubatorCommand(
                Name: request.Name,
                Description: request.Description,
                Key: request.Key),
            cancellationToken);

        if (createResult.IsFailure)
        {
            return Failure(createResult.ErrorCode ?? ResultErrorCodes.Unknown, createResult.ErrorMessages ?? []);
        }

        var incubatorId = createResult.Value.Id;

        var assignResult = await mediator.Send(
            new CreateBusinessIncubatorPackageCommand(
                BusinessIncubatorId: incubatorId,
                PackageVersionId: request.PackageVersionId),
            cancellationToken);

        if (assignResult.IsFailure)
        {
            return Failure(assignResult.ErrorCode ?? ResultErrorCodes.Unknown, assignResult.ErrorMessages ?? []);
        }

        return Success(createResult.Value.ExternalId);
    }
}
