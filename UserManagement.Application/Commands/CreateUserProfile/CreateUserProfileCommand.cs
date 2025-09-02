using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.CreateUserProfile;

public record CreateUserProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Identification,
    string? Email = null) : IBaseRequest<int>;