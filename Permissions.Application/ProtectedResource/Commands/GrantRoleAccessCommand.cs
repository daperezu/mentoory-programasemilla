using FluentValidation;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Commands;

public sealed record GrantRoleAccessCommand(
    long ProtectedResourceId,
    string Role) : IBaseRequest<bool>;

public sealed class GrantRoleAccessCommandValidator : AbstractValidator<GrantRoleAccessCommand>
{
    public GrantRoleAccessCommandValidator()
    {
        RuleFor(x => x.ProtectedResourceId)
            .GreaterThan(0)
            .WithMessage("ProtectedResourceId must be greater than 0.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role cannot be empty.");
    }
}

public sealed class GrantRoleAccessCommandHandler(
    IProtectedResourceRepository protectedResourceRepository,
    IAuditContext auditContext) : BaseCommandHandler<GrantRoleAccessCommand, bool>
{
    public override async Task<Result<bool>> Handle(GrantRoleAccessCommand request, CancellationToken cancellationToken)
    {
        var protectedResource = await protectedResourceRepository.GetProtectedResourceWithPermissionsAsync(request.ProtectedResourceId, cancellationToken);

        if (protectedResource is null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_NotFound, (nameof(request.ProtectedResourceId), "Protected resource not found."));
        }

        protectedResource.GrantAccessToRole(request.Role, auditContext);

        protectedResourceRepository.Update(protectedResource);
        await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(true);
    }
}
