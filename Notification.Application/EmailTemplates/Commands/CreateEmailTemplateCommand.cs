using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;

namespace LinaSys.Notification.Application.EmailTemplates.Commands;

/// <summary>
/// Command to create a new email template.
/// </summary>
public record CreateEmailTemplateCommand(
    string Key,
    string Name,
    string Subject,
    string BodyHtml,
    string? BodyText = null,
    string? Description = null,
    string? Category = null) : IBaseRequest<string>;

/// <summary>
/// Handler for creating email templates.
/// </summary>
public class CreateEmailTemplateCommandHandler(
    IEmailTemplateRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmailTemplateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if template with key already exists
        var existing = await repository.GetByKeyAsync(request.Key);
        if (existing is not null)
        {
            return Result<string>.Failure(
                ResultErrorCodes.GenericError,
                ("Template", $"Un template con la clave '{request.Key}' ya existe"));
        }

        // Create new template
        var template = new EmailTemplate(
            request.Key,
            request.Name,
            request.Subject,
            request.BodyHtml,
            request.BodyText,
            request.Description,
            request.Category);

        repository.Add(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(template.Key);
    }
}