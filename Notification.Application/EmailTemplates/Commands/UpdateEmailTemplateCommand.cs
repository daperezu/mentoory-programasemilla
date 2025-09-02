using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using LinaSys.Shared.Domain.SeedWork;
using MediatR;

namespace LinaSys.Notification.Application.EmailTemplates.Commands;

/// <summary>
/// Command to update an existing email template.
/// </summary>
public record UpdateEmailTemplateCommand(
    string Key,
    string Name,
    string Subject,
    string BodyHtml,
    string? BodyText = null,
    string? Description = null,
    bool? IsActive = null) : LinaSys.Shared.Application.MediatR.IBaseRequest;

/// <summary>
/// Handler for updating email templates.
/// </summary>
public class UpdateEmailTemplateCommandHandler(
    IEmailTemplateRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmailTemplateCommand, Result>
{
    public async Task<Result> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await repository.GetByKeyAsync(request.Key);
        if (template is null)
        {
            return Result.Failure(
                ResultErrorCodes.GenericError,
                ("Template", $"Template con clave '{request.Key}' no encontrado"));
        }

        // Update template properties
        template.Name = request.Name;
        template.UpdateContent(request.Subject, request.BodyHtml, request.BodyText);
        template.Description = request.Description;

        if (request.IsActive.HasValue)
        {
            template.IsActive = request.IsActive.Value;
        }

        repository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}