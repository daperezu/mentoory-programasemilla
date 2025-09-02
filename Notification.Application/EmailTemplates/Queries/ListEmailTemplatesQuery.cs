using LinaSys.Notification.Domain.EmailTemplates;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;
using MediatR;

namespace LinaSys.Notification.Application.EmailTemplates.Queries;

/// <summary>
/// Query to list email templates with optional filtering.
/// </summary>
public record ListEmailTemplatesQuery(
    string? Category = null,
    bool? IsActive = null,
    string? SearchTerm = null) : IBaseRequest<List<EmailTemplateListDto>>;

/// <summary>
/// DTO for email template list item.
/// </summary>
public record EmailTemplateListDto(
    long Id,
    string Key,
    string Name,
    string? Description,
    string? Category,
    bool IsActive,
    DateTime UpdatedAt);

/// <summary>
/// Handler for listing email templates.
/// </summary>
public class ListEmailTemplatesQueryHandler(IEmailTemplateRepository repository) : IRequestHandler<ListEmailTemplatesQuery, Result<List<EmailTemplateListDto>>>
{
    public async Task<Result<List<EmailTemplateListDto>>> Handle(ListEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<EmailTemplate> templates;

        if (!string.IsNullOrEmpty(request.Category))
        {
            templates = await repository.GetByCategoryAsync(request.Category);
        }
        else if (request.IsActive == true)
        {
            templates = await repository.GetActiveTemplatesAsync();
        }
        else
        {
            templates = await repository.GetAllAsync();
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            templates = templates.Where(t =>
                t.Name.ToLower().Contains(searchLower) ||
                t.Key.ToLower().Contains(searchLower) ||
                (t.Description != null && t.Description.ToLower().Contains(searchLower)));
        }

        if (request.IsActive.HasValue)
        {
            templates = templates.Where(t => t.IsActive == request.IsActive.Value);
        }

        var dtos = templates.Select(t => new EmailTemplateListDto(
            t.Id,
            t.Key,
            t.Name,
            t.Description,
            t.Category,
            t.IsActive,
            t.UpdatedAt)).ToList();

        return Result.Success(dtos);
    }
}