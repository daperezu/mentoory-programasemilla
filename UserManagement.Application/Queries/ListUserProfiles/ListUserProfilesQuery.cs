using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.DTOs;

namespace LinaSys.UserManagement.Application.Queries.ListUserProfiles;

public record ListUserProfilesQuery(
    int Start = 0,
    int Length = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IBaseRequest<FilteredQueryResult<UserProfileDto>>;

