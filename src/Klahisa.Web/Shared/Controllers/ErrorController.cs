using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Klahisa.Web.Models;

namespace Klahisa.Web.Shared.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Index()
    {
        var vm = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View("~/Views/Shared/Error.cshtml", vm);
    }
}
