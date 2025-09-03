using LinaSys.Diagnostics.Application.Block.Commands;
using LinaSys.Diagnostics.Application.Block.Queries;
using LinaSys.Web.Areas.Diagnostics.Models.Blocks;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Diagnostics.Controllers;

[Area("Diagnostics")]
public class BlocksController(ILogger<BlocksController> logger, MediatorExecutor mediator)
    : AuthorizedBaseController(logger, mediator)
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListBlocksQuery(
            Start: request.Start,
            Length: request.Length,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            OrderByColumn: request.OrderByColumn ?? "name",
            OrderDirection: request.OrderDirection ?? "asc");

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result!.ToJson(request);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBlockViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBlockViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateBlockCommand(model.Name);
        var result = await MediatorExecutor.SendOrThrowAsync(command);

        this.SetSuccessToast("Bloque creado exitosamente.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var query = new GetBlockByIdQuery(id);
        var result = await MediatorExecutor.SendOrThrowAsync(query);

        var model = new EditBlockViewModel
        {
            Id = result!.Id,
            Name = result.Name,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditBlockViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateBlockCommand(model.Id, model.Name);
        await MediatorExecutor.SendOrThrowAsync(command);

        this.SetSuccessToast("Bloque actualizado exitosamente.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteBlockCommand(id);
        await MediatorExecutor.SendOrThrowAsync(command);

        this.SetSuccessToast("Bloque eliminado exitosamente.");

        return RedirectToAction(nameof(Index));
    }
}
