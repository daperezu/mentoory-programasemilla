using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to restore a deleted Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
public record RestoreBusinessIncubatorCommand(Guid ExternalId) : IBaseRequest;

/// <summary>
/// Validator for <see cref="RestoreBusinessIncubatorCommand"/>.
/// </summary>
public class RestoreBusinessIncubatorCommandValidator : AbstractValidator<RestoreBusinessIncubatorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBusinessIncubatorCommandValidator"/> class.
    /// </summary>
    public RestoreBusinessIncubatorCommandValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="RestoreBusinessIncubatorCommand"/>.
/// </summary>
public class RestoreBusinessIncubatorCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<RestoreBusinessIncubatorCommand>
{
    /// <summary>
    /// Handles the restoration of a deleted Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including deleted entities.
    /// - Checks if the Business Incubator exists.
    /// - Marks the Business Incubator as restored.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or if restoration fails.
    /// </summary>
    public override async Task<Result> Handle(RestoreBusinessIncubatorCommand request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetByExternalIdIncludingDeletedAsync(request.ExternalId, cancellationToken);

        if (incubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound,  (nameof(request.ExternalId), "Business Incubator not found."));
        }

        try
        {
            incubator.SetRestored(auditContext);

            return Success();
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_RestoreFailed, (nameof(RestoreBusinessIncubatorCommand), ex.Message));
        }
    }
}
