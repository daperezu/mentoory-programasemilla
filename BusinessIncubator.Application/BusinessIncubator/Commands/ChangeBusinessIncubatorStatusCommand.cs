using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to change the status of a Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
/// <param name="Status">The new status of the Business Incubator.</param>
public record ChangeBusinessIncubatorStatusCommand(Guid ExternalId, int Status) : IBaseRequest;

/// <summary>
/// Validator for <see cref="ChangeBusinessIncubatorStatusCommand"/>.
/// </summary>
public class ChangeBusinessIncubatorStatusCommandValidator : AbstractValidator<ChangeBusinessIncubatorStatusCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeBusinessIncubatorStatusCommandValidator"/> class.
    /// </summary>
    public ChangeBusinessIncubatorStatusCommandValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().GreaterThan(0);
    }
}

/// <summary>
/// Handler for <see cref="ChangeBusinessIncubatorStatusCommand"/>.
/// </summary>
public class ChangeBusinessIncubatorStatusCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<ChangeBusinessIncubatorStatusCommand>
{
    /// <summary>
    /// Handles the command to change the status of a Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Changes the status of the Business Incubator.
    /// - Saves the changes to the repository.
    /// - Returns a success result if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is deleted.
    /// </summary>
    public override async Task<Result> Handle(ChangeBusinessIncubatorStatusCommand request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.ExternalId), "Business Incubator not found or is deleted."));
        }

        incubator.ChangeStatus((BusinessIncubatorStatus)request.Status, auditContext);

        return Success();
    }
}
