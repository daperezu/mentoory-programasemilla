using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to delete a Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
public record DeleteBusinessIncubatorCommand(Guid ExternalId) : IBaseRequest;

/// <summary>
/// Validator for <see cref="DeleteBusinessIncubatorCommand"/>.
/// </summary>
public class DeleteBusinessIncubatorCommandValidator : AbstractValidator<DeleteBusinessIncubatorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBusinessIncubatorCommandValidator"/> class.
    /// </summary>
    public DeleteBusinessIncubatorCommandValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="DeleteBusinessIncubatorCommand"/>.
/// </summary>
public class DeleteBusinessIncubatorCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<DeleteBusinessIncubatorCommand>
{
    /// <summary>
    /// Handles the deletion of a Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID.
    /// - Checks if the Business Incubator exists and is not already deleted.
    /// - Marks the Business Incubator as deleted.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is already deleted, or if deletion fails.
    /// </summary>
    public override async Task<Result> Handle(DeleteBusinessIncubatorCommand request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound,  (nameof(request.ToString), "Business Incubator not found or already deleted."));
        }

        try
        {
            incubator.SetDeleted(auditContext);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_DeleteFailed, (nameof(DeleteBusinessIncubatorCommand), ex.Message));
        }
    }
}
