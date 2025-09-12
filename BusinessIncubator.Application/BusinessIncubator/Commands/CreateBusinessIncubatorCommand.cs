using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;

/// <summary>
/// Command to create a new Business Incubator.
/// </summary>
/// <param name="Name">The name of the Business Incubator.</param>
/// <param name="Description">The description of the Business Incubator.</param>
/// <param name="Key">The key of the Business Incubator.</param>
public record CreateBusinessIncubatorCommand(string Name, string? Description, string Key) : IBaseRequest<(long Id, Guid ExternalId)>;

/// <summary>
/// Validator for <see cref="CreateBusinessIncubatorCommand"/>.
/// </summary>
public class CreateBusinessIncubatorCommandValidator : AbstractValidator<CreateBusinessIncubatorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBusinessIncubatorCommandValidator"/> class.
    /// </summary>
    public CreateBusinessIncubatorCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
    }
}

/// <summary>
/// Handler for <see cref="CreateBusinessIncubatorCommand"/>.
/// </summary>
public class CreateBusinessIncubatorHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<CreateBusinessIncubatorCommand, (long Id, Guid ExternalId)>
{
    /// <summary>
    /// Handles the creation of a new Business Incubator.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation, including the ID of the created Business Incubator.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Checks if a Business Incubator with the same name already exists.
    /// - Checks if a Business Incubator with the same key already exists.
    /// - Creates a new Business Incubator entity.
    /// - Adds the new Business Incubator to the repository.
    /// - Saves the changes to the repository.
    /// - Returns a success result with the ID of the created Business Incubator if the operation is successful.
    /// - Returns a failure result if a Business Incubator with the same name or key already exists.
    /// </summary>
    public override async Task<Result<(long Id, Guid ExternalId)>> Handle(CreateBusinessIncubatorCommand request, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByNameAsync(request.Name, cancellationToken);

        if (exists)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NameAlreadyExists,  (nameof(request.Name), $"A business incubator with the name '{request.Name}' already exists."));
        }

        exists = await repository.ExistsByKeyAsync(request.Key, cancellationToken);

        if (exists)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_KeyAlreadyExists, (nameof(request.Key), $"A business incubator with the key '{request.Key}' already exists."));
        }

        var incubator = new Domain.Aggregates.BusinessIncubator.BusinessIncubator(request.Name, request.Description, request.Key, auditContext);

        repository.Add(incubator);

        //// Exception to the rule: we don't want to SaveChangesAsync in the handler
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Success((incubator.Id, incubator.ExternalId));
    }
}
