using LinaSys.Notification.Application.EmailTemplates.Commands;
using LinaSys.Notification.Application.EmailTemplates.Queries;
using LinaSys.Shared.Application;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Services;
using LinaSys.Shared.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Coordination.Controllers;

/// <summary>
/// Controller for managing email templates.
/// </summary>
[Area("Coordination")]
[Route("Coordination/[controller]/[action]")]
[Authorize(Roles = "Administrador,Coordinador")]
public class EmailTemplateController(
    IMediator mediator,
    ILogger<EmailTemplateController> logger,
    MediatorExecutor mediatorExecutor,
    IApplicationUrlService applicationUrlService) : AuthorizedBaseController(logger, mediatorExecutor, applicationUrlService)
{

    /// <summary>
    /// Lists all email templates.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Index(string? category = null, bool? isActive = null, string? searchTerm = null)
    {
        var query = new ListEmailTemplatesQuery(category, isActive, searchTerm);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Error al cargar las plantillas de correo");
            return View(new List<EmailTemplateListDto>());
        }

        ViewBag.Category = category;
        ViewBag.IsActive = isActive;
        ViewBag.SearchTerm = searchTerm;

        return View(result.Value);
    }

    /// <summary>
    /// Shows the form to create a new email template.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateEmailTemplateViewModel();
        return View(model);
    }

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmailTemplateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateEmailTemplateCommand(
            model.Key,
            model.Name,
            model.Subject,
            model.BodyHtml,
            model.BodyText,
            model.Description,
            model.Category);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            this.MapErrorsToModelStateAndSetErrorToast<CreateEmailTemplateViewModel>(result);
            return View(model);
        }

        this.SetSuccessToast($"Plantilla '{model.Name}' creada exitosamente");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Shows the form to edit an email template.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Edit(string key)
    {
        var query = new GetEmailTemplateQuery(key);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Plantilla no encontrada");
            return RedirectToAction(nameof(Index));
        }

        var template = result.Value!;
        var model = new EditEmailTemplateViewModel
        {
            Key = template.Key,
            Name = template.Name,
            Subject = template.Subject,
            BodyHtml = template.BodyHtml,
            BodyText = template.BodyText,
            Description = template.Description,
            IsActive = template.IsActive
        };

        return View(model);
    }

    /// <summary>
    /// Updates an email template.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditEmailTemplateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateEmailTemplateCommand(
            model.Key,
            model.Name,
            model.Subject,
            model.BodyHtml,
            model.BodyText,
            model.Description,
            model.IsActive);

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            this.MapErrorsToModelStateAndSetErrorToast<EditEmailTemplateViewModel>(result);
            return View(model);
        }

        this.SetSuccessToast($"Plantilla '{model.Name}' actualizada exitosamente");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Shows email template details.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Details(string key)
    {
        var query = new GetEmailTemplateQuery(key);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            this.SetErrorToast("Plantilla no encontrada");
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>
    /// Preview an email template with sample data.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    [HttpGet]
    public async Task<IActionResult> Preview(string key)
    {
        var query = new GetEmailTemplateQuery(key);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var template = result.Value!;
        var sampleData = GetSampleDataForTemplate(template);
        var renderedHtml = RenderTemplate(template.BodyHtml, sampleData);

        ViewBag.Subject = RenderTemplate(template.Subject, sampleData);
        ViewBag.TemplateName = template.Name;

        return View("Preview", renderedHtml);
    }

    private Dictionary<string, string> GetSampleDataForTemplate(EmailTemplateDto template)
    {
        var sampleData = new Dictionary<string, string>
        {
            { "UserName", "Juan Pérez" },
            { "UserEmail", "juan.perez@ejemplo.com" },
            { "ProjectName", "Proyecto Ejemplo" },
            { "FormName", "Formulario de Inscripción" },
            { "Date", DateTime.Now.ToString("dd/MM/yyyy") },
            { "Time", DateTime.Now.ToString("HH:mm") },
            { "Comments", "Comentarios de ejemplo" },
            { "Link", "https://ejemplo.com/enlace" }
        };

        return sampleData;
    }

    private string RenderTemplate(string template, Dictionary<string, string> data)
    {
        var result = template;
        foreach (var kvp in data)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return result;
    }
}

/// <summary>
/// View model for creating email templates.
/// </summary>
public class CreateEmailTemplateViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// View model for editing email templates.
/// </summary>
public class EditEmailTemplateViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
