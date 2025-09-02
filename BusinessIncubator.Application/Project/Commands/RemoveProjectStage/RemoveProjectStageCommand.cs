using FluentValidation;
using LinaSys.BusinessIncubator.Domain.Enums;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands.RemoveProjectStage;

/// <summary>
/// Command to remove a project stage.
/// </summary>
public sealed record RemoveProjectStageCommand(
    Guid ProjectExternalId,
    ProjectStageType Type) : IBaseRequest<bool>;

/// <summary>
/// Validator for RemoveProjectStageCommand.
/// </summary>
public class RemoveProjectStageCommandValidator : AbstractValidator<RemoveProjectStageCommand>
{
    public RemoveProjectStageCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectExternalId)
            .NotEmpty()
            .WithMessage("El ID del proyecto es requerido.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("El tipo de etapa no es válido.");
    }
}

/// <summary>
/// Handler for RemoveProjectStageCommand.
/// </summary>
public class RemoveProjectStageCommandHandler(
    IBusinessIncubatorRepository repository,
    IAuditContext auditContext)
    : BaseCommandHandler<RemoveProjectStageCommand, bool>
{
    public override async Task<Result<bool>> Handle(
        RemoveProjectStageCommand request,
        CancellationToken cancellationToken)
    {
        // Get the project with stages
        var project = await repository.GetProjectWithStagesByExternalIdAsync(
            request.ProjectExternalId,
            cancellationToken);

        if (project is null || project.IsDeleted)
        {
            return Failure(
                ResultErrorCodes.Project_NotFound,
                (nameof(request.ProjectExternalId), "El proyecto no fue encontrado."));
        }

        try
        {
            // Remove the stage through the aggregate
            project.RemoveStage(request.Type, auditContext);

            // Save changes
            await repository.UpdateAsync(project, cancellationToken);
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Failure(
                ResultErrorCodes.Project_UpdateFailed,
                ("Stage", ex.Message));
        }
    }
}
