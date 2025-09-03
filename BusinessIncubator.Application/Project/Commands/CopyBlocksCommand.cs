using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

public sealed record CopyBlocksCommand(Guid BusinessIncubatorExternalId, Guid ProjectExternalId, List<ProjectBlockToCopyDto> Blocks) : IBaseRequest;

public sealed record ProjectBlockToCopyDto(long? SourceBlockId, string Name);

internal class CopyBlocksCommandValidator : AbstractValidator<CopyBlocksCommand>
{
    public CopyBlocksCommandValidator()
    {
        RuleFor(command => command.BusinessIncubatorExternalId)
            .NotEmpty()
            .WithMessage("Business Incubator External ID must not be empty.");
        RuleFor(command => command.ProjectExternalId)
            .NotEmpty()
            .WithMessage("Project External ID must not be empty.");
        RuleFor(command => command.Blocks)
            .NotEmpty()
            .WithMessage("Blocks list must not be empty.");
    }
}

internal class CopyBlocksCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext) : BaseCommandHandler<CopyBlocksCommand>
{
    public override async Task<Result> Handle(CopyBlocksCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var businessIncubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "Business Incubator not found."));
        }

        // Get the project directly with blocks
        var project = await repository.GetProjectWithBlocksByExternalIdAsync(request.ProjectExternalId, cancellationToken).ConfigureAwait(false);

        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != businessIncubator.Id)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        // Get existing block identifiers from repository
        var (existingNames, existingSourceIds) = await repository.GetProjectBlockIdentifiersAsync(project.Id, cancellationToken).ConfigureAwait(false);

        var blocksToAdd = request.Blocks
            .DistinctBy(block => new { block.SourceBlockId, block.Name })
            .Where(block =>
            {
                // Only check SourceBlockId duplicates if it's not null (matching AddBlock logic)
                var sourceIdCheck = block.SourceBlockId is null || !existingSourceIds.Contains(block.SourceBlockId.Value);

                // Always check for Name duplicates
                var nameCheck = !existingNames.Contains(block.Name);

                return sourceIdCheck && nameCheck;
            })
            .ToList();

        foreach (var block in blocksToAdd)
        {
            project.AddBlock(block.Name, block.SourceBlockId, auditContext);
        }

        repository.Update(project);

        return Success();
    }
}
