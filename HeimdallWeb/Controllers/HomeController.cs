using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HeimdallWeb.Scanners;

namespace HeimdallWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> AcessoRestrito()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Scan(string domainInput)
    {
        ScannerManager scanner = new();

        var result = await scanner.RunAllAsync(domainInput);

        return Content(result.ToString(), "application/json");
    }

}
