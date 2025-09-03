using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public sealed record GetProtectedResourceDetailsQuery(long Id) : IBaseRequest<ProtectedResourceDetailsDto>;

public sealed record ProtectedResourceDetailsDto(
    long Id,
    Guid ExternalId,
    int ResourceType,
    string ResourceTypeName,
    string Name,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    IEnumerable<UserPermissionDto> UserPermissions,
    IEnumerable<RolePermissionDto> RolePermissions);

public sealed record UserPermissionDto(
    long Id,
    string UserId,
    DateTime CreatedAt,
    string CreatedBy);

public sealed record RolePermissionDto(
    long Id,
    string Role,
    DateTime CreatedAt,
    string CreatedBy);
