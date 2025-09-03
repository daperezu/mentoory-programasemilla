using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.DeactivateUser;

public record DeactivateUserCommand(string UserId) : IBaseRequest;

