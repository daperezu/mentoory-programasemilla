using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.BusinessIncubator.Application.Project.Queries;

public record ProjectExistsQuery(Guid BusinessIncubatorExternalId, Guid ProjectExternalId) : IBaseRequest<bool>;

public class ProjectExistsQueryValidator : AbstractValidator<ProjectExistsQuery>
{
    public ProjectExistsQueryValidator()
    {
        RuleFor(query => query.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");
        RuleFor(query => query.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");
    }
}

public class ProjectExistsQueryHandler(IBusinessIncubatorRepository businessIncubatorRepository) : BaseCommandHandler<ProjectExistsQuery, bool>
{
    public override async Task<Result<bool>> Handle(ProjectExistsQuery request, CancellationToken cancellationToken)
    {
        var projectExists = await businessIncubatorRepository.ExistsProjectByExternalIdAsync(request.BusinessIncubatorExternalId, request.ProjectExternalId, cancellationToken).ConfigureAwait(false);
        return Success(projectExists);
    }
}
