using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.ReactivateUser;

public record ReactivateUserCommand(string UserId) : IBaseRequest;

