using System.Diagnostics;
using LinaSys.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinaSys.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
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
