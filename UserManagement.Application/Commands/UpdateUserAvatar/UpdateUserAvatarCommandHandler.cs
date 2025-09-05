using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.UserManagement.Application.Services;
using LinaSys.UserManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LinaSys.UserManagement.Application.Commands.UpdateUserAvatar;

public class UpdateUserAvatarCommandHandler(
    IUserProfileRepository userProfileRepository,
    IAvatarStorageService avatarStorageService,
    ILogger<UpdateUserAvatarCommandHandler> logger)
    : BaseCommandHandler<UpdateUserAvatarCommand, string>
{
    private const int MaxFileSizeInMB = 2;
    private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public override async Task<Result<string>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        // Validate file type
        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            return Failure(ResultErrorCodes.ValidationError, (nameof(request.ContentType), "Tipo de archivo no permitido. Solo se permiten imágenes (JPEG, PNG, GIF, WEBP)"));
        }

        // Validate file size
        if (request.FileStream.Length > MaxFileSizeInBytes)
        {
            return Failure(ResultErrorCodes.ValidationError, (nameof(request.FileStream), $"El archivo excede el tamaño máximo permitido de {MaxFileSizeInMB}MB"));
        }

        var userProfile = await userProfileRepository.GetByUserIdAsync(request.UserId);

        if (userProfile is null)
        {
            return Failure(ResultErrorCodes.Auth_UserNotFound, (nameof(request.UserId), $"Usuario con ID {request.UserId} no encontrado"));
        }

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(userProfile.AvatarUrl))
        {
            await avatarStorageService.DeleteAvatarAsync(userProfile.AvatarUrl, cancellationToken);
        }

        // Upload new avatar to storage
        var avatarUrl = await avatarStorageService.UploadAvatarAsync(
            request.UserId,
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        // Update the user profile with the new avatar URL
        userProfile.UpdateAvatar(avatarUrl);

        userProfileRepository.Update(userProfile);
        await userProfileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        logger.LogInformation("Avatar actualizado para usuario {UserId}: {AvatarUrl}", request.UserId, avatarUrl);

        return Success(avatarUrl);
    }
}

