using System.Diagnostics;
using LinaSys.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If user is not authenticated, redirect to public projects homepage
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Index", "Projects", new { area = "Public" });
        }

        // If authenticated, show the default home page or redirect to dashboard
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        return View();
    }

    [AllowAnonymous]
    public IActionResult PageNotFound()
    {
        Response.StatusCode = 404;
        return View();
    }
}
