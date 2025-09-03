using FluentValidation;
using LinaSys.Permissions.Domain.Aggregates.ProtectedResource;
using LinaSys.Permissions.Domain.Constants;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Commands;

public sealed record CreateProtectedResourceCommand(
    Guid ExternalId,
    int ResourceType,
    string Name,
    string? CreatorUserId = null) : IBaseRequest<long>;

public sealed class CreateProtectedResourceCommandValidator : AbstractValidator<CreateProtectedResourceCommand>
{
    public CreateProtectedResourceCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty()
            .WithMessage("ExternalId cannot be empty.");

        RuleFor(x => x.ResourceType)
            .Must(ResourceTypes.IsValid)
            .WithMessage("ResourceType must be a valid value.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty.")
            .MaximumLength(255)
            .WithMessage("Name cannot exceed 255 characters.");
    }
}

public sealed class CreateProtectedResourceCommandHandler(
    IProtectedResourceRepository protectedResourceRepository,
    IAuditContext auditContext) : BaseCommandHandler<CreateProtectedResourceCommand, long>
{
    public override async Task<Result<long>> Handle(CreateProtectedResourceCommand request, CancellationToken cancellationToken)
    {
        // Check if protected resource already exists for this external ID
        var existingResource = await protectedResourceRepository.GetProtectedResourceByExternalIdAsync(request.ExternalId, cancellationToken).ConfigureAwait(false);
        if (existingResource is not null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_AlreadyExists, (nameof(request.ExternalId), "A protected resource with this ExternalId already exists."));
        }

        // Create the protected resource
        var protectedResource = Domain.Aggregates.ProtectedResource.ProtectedResource.Create(
            request.ExternalId,
            request.ResourceType,
            request.Name,
            auditContext);

        protectedResourceRepository.Add(protectedResource);

        await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Grant access to the creator if specified (after saving to get the ID)
        if (!string.IsNullOrWhiteSpace(request.CreatorUserId))
        {
            protectedResource.GrantAccessToUser(request.CreatorUserId, auditContext);
            await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Success(protectedResource.Id);
    }
}
