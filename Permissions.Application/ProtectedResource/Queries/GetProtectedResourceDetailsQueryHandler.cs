using LinaSys.Permissions.Domain.Constants;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public sealed class GetProtectedResourceDetailsQueryHandler(
    IProtectedResourceRepository protectedResourceRepository) : BaseCommandHandler<GetProtectedResourceDetailsQuery, ProtectedResourceDetailsDto>
{
    public override async Task<Result<ProtectedResourceDetailsDto>> Handle(GetProtectedResourceDetailsQuery request, CancellationToken cancellationToken)
    {
        var protectedResource = await protectedResourceRepository.GetProtectedResourceWithPermissionsAsync(request.Id, cancellationToken);

        if (protectedResource is null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_NotFound, (nameof(request.Id), "Protected resource not found."));
        }

        var userPermissions = protectedResource.UserProtectedResourcePermissions.Select(u => new UserPermissionDto(
            u.Id,
            u.UserId,
            u.CreatedAt,
            u.CreatedBy ?? string.Empty));

        var rolePermissions = protectedResource.RoleProtectedResourcePermissions.Select(r => new RolePermissionDto(
            r.Id,
            r.Role,
            r.CreatedAt,
            r.CreatedBy ?? string.Empty));

        var result = new ProtectedResourceDetailsDto(
            protectedResource.Id,
            protectedResource.ExternalId,
            protectedResource.ResourceType,
            ResourceTypes.GetDisplayName(protectedResource.ResourceType),
            protectedResource.Name,
            protectedResource.CreatedAt,
            protectedResource.CreatedBy ?? string.Empty,
            protectedResource.UpdatedAt,
            protectedResource.UpdatedBy,
            userPermissions,
            rolePermissions);

        return Success(result);
    }
}
