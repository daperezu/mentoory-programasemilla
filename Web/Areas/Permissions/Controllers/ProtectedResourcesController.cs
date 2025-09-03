using LinaSys.Permissions.Application.ProtectedResource.Commands;
using LinaSys.Permissions.Application.ProtectedResource.Queries;
using LinaSys.Web.Areas.Permissions.Models.ProtectedResource;
using LinaSys.Web.Controllers;
using LinaSys.Web.Extensions;
using LinaSys.Web.Models;
using LinaSys.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Areas.Permissions.Controllers;

[Area("Permissions")]
public class ProtectedResourcesController(ILogger<ProtectedResourcesController> logger, MediatorExecutor mediatorExecutor)
    : AuthorizedBaseController(logger, mediatorExecutor)
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(DataTableRequest request)
    {
        var query = new ListProtectedResourcesQuery(
            Start: request.Start,
            Length: request.Length,
            GlobalSearch: request.GlobalSearch,
            Name: request.ColumnSearches.GetValueOrDefault("name"),
            ResourceType: request.ColumnSearches.GetValueOrDefault("resourceType"),
            OrderByColumn: request.OrderByColumn,
            OrderDirection: request.OrderDirection);

        var result = await MediatorExecutor.SendOrThrowAsync(query);

        return result.ToJson(request);
    }

    public async Task<IActionResult> Details(long id)
    {
        var query = new GetProtectedResourceDetailsQuery(id);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(query);
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var details = result.Value!;
        var model = new ProtectedResourceDetailsViewModel
        {
            Id = details.Id,
            ExternalId = details.ExternalId,
            ResourceType = details.ResourceType,
            ResourceTypeName = details.ResourceTypeName,
            Name = details.Name,
            CreatedAt = details.CreatedAt,
            CreatedBy = details.CreatedBy,
            UpdatedAt = details.UpdatedAt,
            UpdatedBy = details.UpdatedBy,
            UserPermissions = details.UserPermissions.Select(u => new UserPermissionViewModel
            {
                Id = u.Id,
                UserId = u.UserId,
                CreatedAt = u.CreatedAt,
                CreatedBy = u.CreatedBy,
            }),
            RolePermissions = details.RolePermissions.Select(r => new RolePermissionViewModel
            {
                Id = r.Id,
                Role = r.Role,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy,
            }),
            GrantUserAccess = new GrantUserAccessViewModel { ResourceId = details.Id },
            GrantRoleAccess = new GrantRoleAccessViewModel { ResourceId = details.Id },
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateName(long id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            this.SetErrorToast("Name is required.");
            return RedirectToAction(nameof(Details), new { id });
        }

        var command = new UpdateProtectedResourceCommand(id, name);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
        if (!result.IsSuccess)
        {
            MapErrorsToModelStateAndSetErrorToast<UpdateProtectedResourceCommand>(result);
        }
        else
        {
            this.SetSuccessToast("Protected resource name updated successfully.");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantUserAccess(GrantUserAccessViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var command = new GrantUserAccessCommand(viewModel.ResourceId, viewModel.UserId);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
            if (result.IsSuccess)
            {
                this.SetSuccessToast($"Access granted to user {viewModel.UserId}.");
                return RedirectToAction(nameof(Details), new { id = viewModel.ResourceId });
            }

            MapErrorsToModelStateAndSetErrorToast<GrantUserAccessCommand>(result);
        }

        return RedirectToAction(nameof(Details), new { id = viewModel.ResourceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeUserAccess(long id, string userId)
    {
        var command = new RevokeUserAccessCommand(id, userId);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
        if (!result.IsSuccess)
        {
            MapErrorsToModelStateAndSetErrorToast<RevokeUserAccessCommand>(result);
        }
        else
        {
            this.SetSuccessToast($"Access revoked from user {userId}.");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantRoleAccess(GrantRoleAccessViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var command = new GrantRoleAccessCommand(viewModel.ResourceId, viewModel.Role);
            var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
            if (result.IsSuccess)
            {
                this.SetSuccessToast($"Access granted to role {viewModel.Role}.");
                return RedirectToAction(nameof(Details), new { id = viewModel.ResourceId });
            }

            MapErrorsToModelStateAndSetErrorToast<GrantRoleAccessCommand>(result);
        }

        return RedirectToAction(nameof(Details), new { id = viewModel.ResourceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeRoleAccess(long id, string role)
    {
        var command = new RevokeRoleAccessCommand(id, role);
        var result = await MediatorExecutor.SendAndLogIfFailureAsync(command);
        if (!result.IsSuccess)
        {
            MapErrorsToModelStateAndSetErrorToast<RevokeRoleAccessCommand>(result);
        }
        else
        {
            this.SetSuccessToast($"Access revoked from role {role}.");
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
