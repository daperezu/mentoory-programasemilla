using LinaSys.Auth.Application.Queries.Context;
using LinaSys.BusinessIncubator.Application.Queries;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Orchestration.Application.UserContext.Commands;

public record GetEnrichedUserContextCommand(string UserId) : IBaseRequest<EnrichedUserContextDto?>;

public record EnrichedUserContextDto(
    string UserId,
    string? Role,
    long? IncubatorId,
    string? IncubatorName,
    long? ProjectId,
    string? ProjectName,
    bool IsGlobalAdministrator);

public class GetEnrichedUserContextCommandHandler(IMediator mediator) : BaseCommandHandler<GetEnrichedUserContextCommand, EnrichedUserContextDto?>
{
    public override async Task<Result<EnrichedUserContextDto?>> Handle(
        GetEnrichedUserContextCommand request,
        CancellationToken cancellationToken)
    {
        // Get the enriched user context from Auth domain (includes role name)
        var contextResult = await mediator.Send(
            new GetEnrichedLastUserContextQuery(request.UserId),
            cancellationToken);

        if (!contextResult.IsSuccess || contextResult.Value == null)
        {
            return Success(null);
        }

        var authContext = contextResult.Value;

        // Get incubator name using query
        string? incubatorName = null;
        if (authContext.IncubatorId.HasValue)
        {
            var incubatorResult = await mediator.Send(
                new GetIncubatorByIdQuery(authContext.IncubatorId.Value),
                cancellationToken);

            incubatorName = incubatorResult.Value?.Name;
        }

        // Get project name using query
        string? projectName = null;
        if (authContext.ProjectId.HasValue)
        {
            var projectResult = await mediator.Send(
                new GetProjectByIdQuery(authContext.ProjectId.Value),
                cancellationToken);

            projectName = projectResult.Value?.Name;
        }

        // Create enriched DTO with business domain data
        var enrichedContext = new EnrichedUserContextDto(
            authContext.UserId,
            authContext.Role,
            authContext.IncubatorId,
            incubatorName,          // Enriched from BusinessIncubator domain
            authContext.ProjectId,
            projectName,            // Enriched from BusinessIncubator domain
            authContext.IsGlobalAdministrator);

        return Success(enrichedContext);
    }
}
