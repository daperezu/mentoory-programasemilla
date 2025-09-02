using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Permissions.Application.ProtectedResource.Queries;

public sealed record ListProtectedResourcesQuery(
    int Start = 0,
    int Length = 50,
    string? GlobalSearch = null,
    string? Name = null,
    string? ResourceType = null,
    string? OrderByColumn = null,
    string? OrderDirection = null) : IBaseRequest<FilteredQueryResult<ListProtectedResourceDto>>;

public sealed record ListProtectedResourceDto(
    long Id,
    Guid ExternalId,
    int ResourceType,
    string ResourceTypeName,
    string Name,
    DateTime CreatedAt,
    string CreatedBy,
    int UserPermissionsCount,
    int RolePermissionsCount);
