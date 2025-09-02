using LinaSys.Shared.Application.MediatR;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserAvatar;

public record UpdateUserAvatarCommand(
    string UserId,
    Stream FileStream,
    string FileName,
    string ContentType) : IBaseRequest<string>;

