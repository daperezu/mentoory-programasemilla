using FluentValidation;
using FluentValidation.Validators;
using LinaSys.BusinessIncubator.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.BusinessIncubator.Application.Project.Commands;

public sealed record CopyKnowledgeStructureCommand(
    Guid BusinessIncubatorExternalId,
    Guid ProjectExternalId,
    long? SourceKnowledgeStructureId,
    string Name,
    string? Description,
    List<KnowledgeStructureModuleToCopyDto> Modules)
    : IBaseRequest;

public sealed record KnowledgeStructureModuleToCopyDto(long? SourceModuleId, string Name, int Order, List<KnowledgeStructureTopicToCopyDto> Topics);

public sealed record KnowledgeStructureTopicToCopyDto(long? SourceTopicId, string Name, int Order, List<KnowledgeStructureSubjectToCopyDto> Subjects);

public sealed record KnowledgeStructureSubjectToCopyDto(long? SourceSubjectId, string Title, string? Content, int Order, List<KnowledgeStructureSubjectResourceToCopyDto> SubjectResources);

public sealed record KnowledgeStructureSubjectResourceToCopyDto(long? SourceSubjectResourceId, string Title, string Url, string Type, int? EstimatedMinutes, int Order);

internal class CopyProjectKnowledgeStructureCommandValidator : AbstractValidator<CopyKnowledgeStructureCommand>
{
    public CopyProjectKnowledgeStructureCommandValidator()
    {
        RuleFor(x => x.BusinessIncubatorExternalId)
            .NotEmpty().WithMessage("Business Incubator External ID is required.");
        RuleFor(x => x.ProjectExternalId)
            .NotEmpty().WithMessage("Project External ID is required.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Knowledge Structure Name is required.")
            .MaximumLength(100).WithMessage("Knowledge Structure Name must not exceed 100 characters.");
        RuleFor(x => x.Modules)
            .NotNull().WithMessage("Knowledge Structure Modules cannot be null.")
            .Must(modules => modules.All(m => !string.IsNullOrEmpty(m.Name)))
            .WithMessage("All Knowledge Structure Modules must have a Name.")
            .When(x => x.Modules is not null && x.Modules.Any());

        RuleForEach(x => x.Modules).
            SetValidator(new ProjectKnowledgeStructureModuleDtoValidator());
    }
}

internal class ProjectKnowledgeStructureModuleDtoValidator : IPropertyValidator<CopyKnowledgeStructureCommand, KnowledgeStructureModuleToCopyDto>
{
    public string Name { get; }

    public bool IsValid(ValidationContext<CopyKnowledgeStructureCommand> context, KnowledgeStructureModuleToCopyDto value)
    {
        if (string.IsNullOrEmpty(value.Name))
        {
            context.AddFailure("Knowledge Structure Module Name is required.");
            return false;
        }

        if (value.Name.Length > 100)
        {
            context.AddFailure("Knowledge Structure Module Name must not exceed 100 characters.");
            return false;
        }

        // Topics are optional - modules can exist without topics
        if (value.Topics.Any() && value.Topics.Any(topic => string.IsNullOrEmpty(topic.Name)))
        {
            context.AddFailure("All Knowledge Structure Topics must have a Name.");
            return false;
        }

        return true;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return "Invalid Knowledge Structure Module data.";
    }
}

internal sealed class CopyProjectKnowledgeStructureCommandHandler(IBusinessIncubatorRepository repository, IAuditContext auditContext)
    : BaseCommandHandler<CopyKnowledgeStructureCommand>
{
    public override async Task<Result> Handle(CopyKnowledgeStructureCommand request, CancellationToken cancellationToken)
    {
        // First verify the business incubator exists
        var businessIncubator = await repository.GetWithProjectsByExternalIdAsync(request.BusinessIncubatorExternalId, cancellationToken).ConfigureAwait(false);

        if (businessIncubator is null)
        {
            return Failure(ResultErrorCodes.BusinessIncubator_NotFound, (nameof(request.BusinessIncubatorExternalId), "BusinessIncubator not found."));
        }

        businessIncubator.EnsureNotDeleted();

        // Get the project with knowledge structure
        var project = await repository.GetProjectWithKnowledgeStructureByExternalIdAsync(request.ProjectExternalId, cancellationToken).ConfigureAwait(false);

        if (project is null)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project not found."));
        }

        // Verify the project belongs to the business incubator
        if (project.BusinessIncubatorId != businessIncubator.Id)
        {
            return Failure(ResultErrorCodes.Project_NotFound, (nameof(request.ProjectExternalId), "Project does not belong to the specified Business Incubator."));
        }

        // Check if knowledge structure already exists by attempting to set it
        // The SetKnowledgeStructure method will handle the case where it already exists
        var knowledgeStructure = project.SetKnowledgeStructure(
            request.SourceKnowledgeStructureId,
            request.Name,
            false,
            request.Description,
            false,
            auditContext);

        foreach (var moduleToCopy in request.Modules)
        {
            var module = knowledgeStructure.AddProjectModule(moduleToCopy.SourceModuleId, moduleToCopy.Name, false, moduleToCopy.Order, false);

            foreach (var topicToCopy in moduleToCopy.Topics)
            {
                var topic = module.AddProjectTopic(topicToCopy.SourceTopicId, topicToCopy.Name, false, topicToCopy.Order, false);

                foreach (var subjectToCopy in topicToCopy.Subjects)
                {
                    var subject = topic.AddProjectSubject(
                        subjectToCopy.SourceSubjectId,
                        subjectToCopy.Title,
                        false,
                        subjectToCopy.Content,
                        false,
                        subjectToCopy.Order,
                        false);

                    foreach (var resourceToCopy in subjectToCopy.SubjectResources)
                    {
                        subject.AddProjectSubjectResource(
                            resourceToCopy.SourceSubjectResourceId,
                            resourceToCopy.Title,
                            false,
                            resourceToCopy.Url,
                            false,
                            resourceToCopy.Type,
                            false,
                            resourceToCopy.EstimatedMinutes,
                            false,
                            resourceToCopy.Order,
                            false);
                    }
                }
            }
        }

        repository.Update(project);

        return Success();
    }
}
