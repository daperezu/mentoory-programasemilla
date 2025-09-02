using LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;
using LinaSys.BusinessIncubator.Application.BusinessIncubator.Queries;
using LinaSys.Orchestration.Application.BusinessIncubator.Commands;
using LinaSys.Orchestration.Application.BusinessIncubator.Queries;
using LinaSys.Subscription.Application.Package.Queries;
using LinaSys.Web.Areas.BusinessIncubators.Models.BusinessIncubator;
using LinaSys.Web.Attributes;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinaSys.Web.Areas.BusinessIncubators.Controllers;

[Area("BusinessIncubators")]
public class HomeController(ILogger<HomeController> logger, MediatorExecutor mediator) : AuthorizedBaseController(logger, mediator)
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExtraLimit(AddExtraLimitViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var command = new AddBusinessIncubatorExtraLimitCommand(viewModel.Id, viewModel.Type, viewModel.Quantity);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                this.SetSuccessToast("Se agregó el límite extra.");
                return RedirectToAction("Edit", new { id = viewModel.Id, tab = EditViewModel.SubscriptionTabId });
            }

            MapErrorsToModelStateAndSetErrorToast<AddBusinessIncubatorExtraLimitCommand>(result);
        }

        return RedirectToAction("Edit", new { id = viewModel.Id, tab = EditViewModel.SubscriptionTabId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAllLimits(Guid id, CancellationToken cancellationToken)
    {
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(new ClearBusinessIncubatorExtraLimitsCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            this.SetSuccessToast("Se eliminaron todos los límites extra.");
        }
        else
        {
            MapErrorsToModelStateAndSetErrorToast<ClearBusinessIncubatorExtraLimitsCommand>(result);
        }

        return RedirectToAction("Edit", new { id, tab = EditViewModel.SubscriptionTabId });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateViewModel { PackageOptions = await GetPackageOptionsAsync(cancellationToken), };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var command = new CreateBusinessIncubatorWithPackageCommand(
            viewModel.Name,
            viewModel.Description,
            viewModel.Key,
            viewModel.PackageVersionId!.Value);

        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
        if (result.IsSuccess)
        {
            this.SetSuccessToast("Se creó la incubadora.");
            return RedirectToAction("Edit", new { id = result.Value });
        }

        MapErrorsToModelStateAndSetErrorToast<CreateBusinessIncubatorWithPackageCommand>(result);

        viewModel.PackageOptions = await GetPackageOptionsAsync(cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteBusinessIncubatorCommand(id);

        await MediatorExecutor.SendOrThrowAsync(command, cancellationToken);

        this.SetSuccessToast("Se eliminó la incubadora.");
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExtraLimit(DeleteExtraLimitViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(new DeleteBusinessIncubatorExtraLimitCommand(viewModel.Id, viewModel.Type, viewModel.Quantity), cancellationToken);
            if (result.IsSuccess)
            {
                this.SetSuccessToast("Se eliminó la extensión al límite.");
            }
            else
            {
                MapErrorsToModelStateAndSetErrorToast<DeleteBusinessIncubatorExtraLimitCommand>(result);
            }
        }

        return RedirectToAction("Edit", new { viewModel.Id, tab = EditViewModel.SubscriptionTabId });
    }

    [HttpGet]
    [RestoreModelAndState<EditViewModel>]
    public async Task<IActionResult> Edit(EditViewModel viewModel, string tab = EditViewModel.DetailsTabId, CancellationToken cancellationToken = default)
    {
        await RefreshEditViewModelAsync(viewModel, cancellationToken);
        ViewBag.Tab = tab;
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Landing()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListBusinessIncubatorsQuery(
            Start: request.Start,
            Length: request.Length,
            GlobalSearch: request.GlobalSearch,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            Description: request.ColumnSearches.GetValueOrDefault("description"),
            Key: request.ColumnSearches.GetValueOrDefault("key"),
            StatusId: int.TryParse(request.ColumnSearches.GetValueOrDefault("status"), out var s) ? s : null,
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDetails(UpdateDetailsViewModel viewViewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var command = new UpdateBusinessIncubatorCommand(viewViewModel.Id, viewViewModel.Name, viewViewModel.Description, viewViewModel.Key);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                this.SetSuccessToast("Se actualizaron los datos.");
                return RedirectToAction("Edit", new { id = viewViewModel.Id, tab = EditViewModel.DetailsTabId });
            }

            MapErrorsToModelStateAndSetErrorToast<UpdateBusinessIncubatorCommand>(result);
        }

        return RedirectToActionWithModel("Edit", new { id = viewViewModel.Id, tab = EditViewModel.DetailsTabId }, viewViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateStatusViewModel viewViewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var command = new ChangeBusinessIncubatorStatusCommand(viewViewModel.Id, viewViewModel.Status);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                this.SetSuccessToast("Se actualizó el estado.");
                return RedirectToAction("Edit", new { id = viewViewModel.Id, tab = EditViewModel.StatusTabId });
            }

            MapErrorsToModelStateAndSetErrorToast<UpdateStatusViewModel>(result);
        }

        return RedirectToActionWithModel("Edit", new { id = viewViewModel.Id, tab = EditViewModel.StatusTabId }, viewViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSubscription(UpdateSubscriptionViewModel viewViewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var command = new SwitchBusinessIncubatorPackageVersionCommand(viewViewModel.Id, viewViewModel.PackageVersionId);

            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                this.SetSuccessToast("Se actualizó la subscripción.");
                return RedirectToAction("Edit", new { id = viewViewModel.Id, tab = EditViewModel.SubscriptionTabId });
            }

            MapErrorsToModelStateAndSetErrorToast<SwitchBusinessIncubatorPackageVersionCommand>(result);
        }

        return RedirectToActionWithModel("Edit", new { id = viewViewModel.Id, tab = EditViewModel.SubscriptionTabId }, viewViewModel);
    }

    private Task<List<SelectListItem>> GetPackageLimitTypeOptionsAsync(CancellationToken cancellationToken)
    {
        var result = new List<SelectListItem> { new("Proyectos", "1"), new("Usuarios", "2"), };

        return Task.FromResult(result);
    }

    private async Task<List<SelectListItem>> GetPackageOptionsAsync(CancellationToken cancellationToken)
    {
        var versions = await MediatorExecutor.SendOrThrowAsync(new GetAvailablePackageVersionsQuery(), cancellationToken);

        return versions
            .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Label, }).ToList();
    }

    private Task<List<SelectListItem>> GetStatusOptionsAsync(CancellationToken cancellationToken)
    {
        var statuses = new (int Id, string Name)[] { (1, "Activo"), (2, "Archivado") };

        var items = statuses
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name, }).ToList();

        return Task.FromResult(items);
    }

    private async Task RefreshEditViewModelAsync(EditViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!viewModel.WasRestored)
        {
            var result = await MediatorExecutor.SendOrThrowAsync(
                new GetBusinessIncubatorDetailsWithSubscriptionPackageQuery(viewModel.Id), cancellationToken);

            viewModel.Name = result.BusinessIncubatorDetails.Name;
            viewModel.Description = result.BusinessIncubatorDetails.Description;
            viewModel.Key = result.BusinessIncubatorDetails.Key;
            viewModel.Status = result.BusinessIncubatorDetails.Status;
            viewModel.PackageVersionId = result.PackageAndLimits.PackageVersionId;
            viewModel.PackageLimits = result.PackageAndLimits.PackageLimits;
            viewModel.LimitOverrides = result.PackageAndLimits.LimitOverrides;
            viewModel.EffectiveLimits = result.PackageAndLimits.EffectiveLimits;

            ModelState.Clear();
        }

        viewModel.PackageOptions = await GetPackageOptionsAsync(cancellationToken);
        viewModel.StatusOptions = await GetStatusOptionsAsync(cancellationToken);
        viewModel.PackageLimitTypes = await GetPackageLimitTypeOptionsAsync(cancellationToken);
    }
}
