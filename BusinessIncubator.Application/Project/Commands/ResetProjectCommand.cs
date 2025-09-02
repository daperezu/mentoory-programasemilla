using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

public sealed record ResetProjectCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    bool ResetBlocks = true,
    bool ResetKnowledgeStructure = true) : IBaseRequest;

public class ResetProjectCommandValidator : AbstractValidator<ResetProjectCommand>
{
    public ResetProjectCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");

        RuleFor(command => command.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");

        RuleFor(command => command)
            .Must(cmd => cmd.ResetBlocks || cmd.ResetKnowledgeStructure)
            .WithMessage("At least one reset option (ResetBlocks or ResetKnowledgeStructure) must be true.");
    }
}

public class ResetProjectCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<ResetProjectCommand>
{
    public override async Task<Result> Handle(ResetProjectCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var businessIncubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return Failure(
                ResultErrorCodes.BusinessIncubator_NotFound,
                (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found."));
        }

        businessIncubator.EnsureNotDeleted();

        // Get the project with knowledge structure
        var project = await repository.GetProjectWithKnowledgeStructureByExternalIdAsync(request.ProjectExternalId, cancellationToken).ConfigureAwait(false);

        if (project is null)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != businessIncubator.Id)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        if (request.ResetBlocks && request.ResetKnowledgeStructure)
        {
            project.ResetProjectContent(auditContext);
        }
        else if (request.ResetBlocks)
        {
            project.ClearAllBlocks(auditContext);
        }
        else if (request.ResetKnowledgeStructure)
        {
            project.ClearKnowledgeStructure(auditContext);
        }

        repository.Update(project);

        return Success();
    }
}
