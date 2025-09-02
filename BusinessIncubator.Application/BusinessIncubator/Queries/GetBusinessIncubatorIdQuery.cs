using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;

/// <summary>
/// Query to get the details of a Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
public record GetBusinessIncubatorIdQuery(Guid ExternalId) : IBaseRequest<long>;

/// <summary>
/// Validator for <see cref="GetBusinessIncubatorIdQuery"/>.
/// </summary>
public class GetBusinessIncubatorIdQueryValidator : AbstractValidator<GetBusinessIncubatorIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBusinessIncubatorIdQueryValidator"/> class.
    /// </summary>
    public GetBusinessIncubatorIdQueryValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
    }
}

/// <summary>
/// Handler for <see cref="GetBusinessIncubatorIdQuery"/>.
/// </summary>
public class GetBusinessIncubatorIdQueryHandler(IBusinessIncubatorRepository repository)
    : BaseCommandHandler<GetBusinessIncubatorIdQuery, long>
{
    public override async Task<Result<long>> Handle(GetBusinessIncubatorIdQuery request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetByExternalIdAsync(request.ExternalId, cancellationToken);

        if (incubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.ExternalId), "Business Incubator not found."));
        }

        return Success(incubator.Id);
    }
}
