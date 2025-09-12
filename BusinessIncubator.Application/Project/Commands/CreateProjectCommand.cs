using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

public record CreateProjectCommand(Guid BusinessIncubatorExternalId, string Name, string? Description, string Key) : IBaseRequest<Guid>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.BusinessIncubatorExternalId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(1000);
    }
}

public class CreateProjectCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<CreateProjectCommand, Guid>
{
    public override async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var incubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken);

        if (incubator is null || incubator.IsDeleted)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found or is deleted."));
        }

        // Check for duplicate name
        if (await repository.ProjectExistsWithNameAsync(incubator.Id, request.Name, null, cancellationToken))
        {
            return Failure(ResultErrorCodes.Project_CreationFailed, (nameof(request.Name), "Ya existe un proyecto con el mismo nombre."));
        }

        // Check for duplicate key
        if (await repository.ProjectExistsWithKeyAsync(incubator.Id, request.Key, null, cancellationToken))
        {
            return Failure(ResultErrorCodes.Project_CreationFailed, (nameof(request.Key), "Ya existe un proyecto con la misma clave."));
        }

        try
        {
            // Create the project directly
            var project = new Domain.Aggregates.BusinessIncubator.Project(
                request.Name,
                request.Description,
                request.Key,
                incubator.Id,
                auditContext);

            // Add project to repository
            await repository.AddProjectAsync(project, cancellationToken);

            return Result.Success(project.ExternalId);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ResultErrorCodes.Project_CreationFailed, (nameof(CreateProjectCommand), ex.Message));
        }
    }
}
