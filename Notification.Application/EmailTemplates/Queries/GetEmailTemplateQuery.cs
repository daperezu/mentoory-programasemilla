using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Notification.Application.EmailTemplates.Queries;

/// <summary>
/// Query to get an email template by key.
/// </summary>
public record GetEmailTemplateQuery(string Key) : IBaseRequest<EmailTemplateDto>;

/// <summary>
/// DTO for email template.
/// </summary>
public record EmailTemplateDto(
    long Id,
    string Key,
    string Name,
    string Subject,
    string BodyHtml,
    string? BodyText,
    string? Description,
    string? Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Handler for getting email template by key.
/// </summary>
public class GetEmailTemplateQueryHandler(IEmailTemplateRepository repository) : IRequestHandler<GetEmailTemplateQuery, Result<EmailTemplateDto>>
{
    public async Task<Result<EmailTemplateDto>> Handle(GetEmailTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await repository.GetByKeyAsync(request.Key);
        if (template is null)
        {
            return Result<EmailTemplateDto>.Failure(
                ResultErrorCodes.GenericError,
                ("Template", $"Template con clave '{request.Key}' no encontrado"));
        }

        var dto = new EmailTemplateDto(
            template.Id,
            template.Key,
            template.Name,
            template.Subject,
            template.BodyHtml,
            template.BodyText,
            template.Description,
            template.Category,
            template.IsActive,
            template.CreatedAt,
            template.UpdatedAt);

        return Result.Success(dto);
    }
}
