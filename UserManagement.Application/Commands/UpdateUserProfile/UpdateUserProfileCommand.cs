using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(
    string UserId,
    string FirstName,
    string LastName) : IBaseRequest;