using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to update an existing Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
/// <param name="Name">The name of the Business Incubator.</param>
/// <param name="Description">The description of the Business Incubator.</param>
/// <param name="Key">The key of the Business Incubator.</param>
public record UpdateBusinessIncubatorCommand(Guid ExternalId, string Name, string? Description, string Key) : IBaseRequest;

/// <summary>
/// Validator for <see cref="UpdateBusinessIncubatorCommand"/>.
/// </summary>
public class UpdateBusinessIncubatorCommandValidator : AbstractValidator<UpdateBusinessIncubatorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBusinessIncubatorCommandValidator"/> class.
    /// </summary>
    public UpdateBusinessIncubatorCommandValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
    }
}

/// <summary>
/// Handler for <see cref="UpdateBusinessIncubatorCommand"/>.
/// </summary>
public class UpdateBusinessIncubatorHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<UpdateBusinessIncubatorCommand>
{
    /// <summary>
    /// Handles the update of an existing Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Checks if a Business Incubator with the same name already exists, excluding the current entity.
    /// - Checks if a Business Incubator with the same key already exists, excluding the current entity.
    /// - Retrieves the Business Incubator by its external ID.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Updates the details of the Business Incubator.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if a Business Incubator with the same name or key already exists, or if the Business Incubator is not found.
    /// </summary>
    public override async Task<Result> Handle(UpdateBusinessIncubatorCommand request, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByNameNotItselfAsync(request.ExternalId, request.Name, cancellationToken);

        if (exists)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NameAlreadyExists, (nameof(request.Name), $"A business incubator with the name '{request.Name}' already exists."));
        }

        exists = await repository.ExistsByKeyNotItselfAsync(request.ExternalId, request.Key, cancellationToken);

        if (exists)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_KeyAlreadyExists, (nameof(request.Key), $"A business incubator with the key '{request.Key}' already exists."));
        }

        var incubator = await repository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

        if (incubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.ExternalId), $"A business incubator with the external id '{request.ExternalId:B}' was not found."));
        }

        incubator.EnsureNotDeleted();

        incubator.Update(request.Name, request.Description, request.Key, auditContext);

        return Success();
    }
}
