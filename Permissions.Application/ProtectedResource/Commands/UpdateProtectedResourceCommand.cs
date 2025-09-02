using FluentValidation;
using LinaSys.Permissions.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Permissions.Application.ProtectedResource.Commands;

public sealed record UpdateProtectedResourceCommand(
    long Id,
    string Name) : IBaseRequest<bool>;

public sealed class UpdateProtectedResourceCommandValidator : AbstractValidator<UpdateProtectedResourceCommand>
{
    public UpdateProtectedResourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty.")
            .MaximumLength(255)
            .WithMessage("Name cannot exceed 255 characters.");
    }
}

public sealed class UpdateProtectedResourceCommandHandler(
    IProtectedResourceRepository protectedResourceRepository,
    IAuditContext auditContext) : BaseCommandHandler<UpdateProtectedResourceCommand, bool>
{
    public override async Task<Result<bool>> Handle(UpdateProtectedResourceCommand request, CancellationToken cancellationToken)
    {
        var protectedResource = await protectedResourceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (protectedResource is null)
        {
            return Failure(ResultErrorCodes.ProtectedResource_NotFound, (nameof(request.Id), "Protected resource not found."));
        }

        protectedResource.UpdateName(request.Name, auditContext);

        protectedResourceRepository.Update(protectedResource);
        await protectedResourceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success(true);
    }
}
