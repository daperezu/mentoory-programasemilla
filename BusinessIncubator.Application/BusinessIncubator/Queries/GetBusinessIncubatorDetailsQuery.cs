using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;

/// <summary>
/// Query to get the details of a Business Incubator.
/// </summary>
/// <param name="ExternalId">The external ID of the Business Incubator.</param>
public record GetBusinessIncubatorDetailsQuery(Guid ExternalId) : IBaseRequest<BusinessIncubatorDetailsDto>;

/// <summary>
/// Validator for <see cref="GetBusinessIncubatorDetailsQuery"/>.
/// </summary>
public class GetBusinessIncubatorDetailsQueryValidator : AbstractValidator<GetBusinessIncubatorDetailsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBusinessIncubatorDetailsQueryValidator"/> class.
    /// </summary>
    public GetBusinessIncubatorDetailsQueryValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
    }
}

public record BusinessIncubatorDetailsDto(long Id, string Name, string? Description, string Key, int Status);

/// <summary>
/// Handler for <see cref="GetBusinessIncubatorDetailsQuery"/>.
/// </summary>
public class GetBusinessIncubatorDetailsQueryHandler(IBusinessIncubatorRepository repository)
    : BaseCommandHandler<GetBusinessIncubatorDetailsQuery, BusinessIncubatorDetailsDto>
{
    /// <summary>
    /// Handles the query to get the details of a Business Incubator.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the Business Incubator details or an error.</returns>
    /// <summary>
    /// Implementation Details:
    /// - Retrieves the Business Incubator by its external ID, including its projects.
    /// - Checks if the Business Incubator exists and is not deleted.
    /// - Maps the Business Incubator and its projects to the corresponding DTOs.
    /// - Returns a success result with the Business Incubator details if the operation is successful.
    /// - Returns a failure result if the Business Incubator is not found or is deleted.
    /// </summary>
    public override async Task<Result<BusinessIncubatorDetailsDto>> Handle(GetBusinessIncubatorDetailsQuery request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.ExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.ExternalId), "Business Incubator not found or is deleted."));
        }

        var dto = new BusinessIncubatorDetailsDto(
            incubator.Id,
            incubator.Name,
            incubator.Description,
            incubator.Key,
            (int)incubator.Status);

        return Success(dto);
    }
}
