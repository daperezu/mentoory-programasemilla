using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.DTOs;

namespace LinaSys.UserManagement.Application.Queries.GetUserProfileByUserId;

public record GetUserProfileByUserIdQuery(string UserId) : IBaseRequest<UserProfileDto>;