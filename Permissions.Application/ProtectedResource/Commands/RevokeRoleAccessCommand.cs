using FluentValidation;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Commands;

public sealed record RevokeRoleAccessCommand(
    long ProtectedResourceId,
    string Role) : IBaseRequest<bool>;

public sealed class RevokeRoleAccessCommandValidator : AbstractValidator<RevokeRoleAccessCommand>
{
    public RevokeRoleAccessCommandValidator()
    {
        RuleFor(x => x.ProtectedResourceId)
            .GreaterThan(0)
            .WithMessage("ProtectedResourceId must be greater than 0.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role cannot be empty.");
    }
}

public sealed class RevokeRoleAccessCommandHandler(
    IProtectedResourceRepository protectedResourceRepository,
    IAuditContext auditContext) : BaseCommandHandler<RevokeRoleAccessCommand, bool>
{
    public override async Task<Result<bool>> Handle(RevokeRoleAccessCommand request, CancellationToken cancellationToken)
    {
        var protectedResource = await protectedResourceRepository.GetProtectedResourceWithPermissionsAsync(request.ProtectedResourceId, cancellationToken);

        if (protectedResource is null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_NotFound, (nameof(request.ProtectedResourceId), "Protected resource not found."));
        }

        protectedResource.RevokeAccessFromRole(request.Role, auditContext);

        protectedResourceRepository.Update(protectedResource);
        await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(true);
    }
}
