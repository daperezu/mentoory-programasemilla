using FluentValidation;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Commands;

public sealed record GrantUserAccessCommand(
    long ProtectedResourceId,
    string UserId) : IBaseRequest<bool>;

public sealed class GrantUserAccessCommandValidator : AbstractValidator<GrantUserAccessCommand>
{
    public GrantUserAccessCommandValidator()
    {
        RuleFor(x => x.ProtectedResourceId)
            .GreaterThan(0)
            .WithMessage("ProtectedResourceId must be greater than 0.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");
    }
}

public sealed class GrantUserAccessCommandHandler(
    IProtectedResourceRepository protectedResourceRepository,
    IAuditContext auditContext) : BaseCommandHandler<GrantUserAccessCommand, bool>
{
    public override async Task<Result<bool>> Handle(GrantUserAccessCommand request, CancellationToken cancellationToken)
    {
        var protectedResource = await protectedResourceRepository.GetProtectedResourceWithPermissionsAsync(request.ProtectedResourceId, cancellationToken);

        if (protectedResource is null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_NotFound, (nameof(request.ProtectedResourceId), "Protected resource not found."));
        }

        protectedResource.GrantAccessToUser(request.UserId, auditContext);

        protectedResourceRepository.Update(protectedResource);
        await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(true);
    }
}
