using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HeimdallWeb.Controllers;

public class HomeController : Controller
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository;
    private readonly IConfiguration _config;
    private readonly IScanService _scanService;

    public HomeController(
        IHistoryRepository historyRepository
        , IConfiguration config
        , IFindingRepository findingRepository
        , IScanService scanService)
    {
        _historyRepository = historyRepository;
        _config = config;
        _findingRepository = findingRepository;
        _scanService = scanService;
    }

    public IActionResult Index([FromQuery] int? rateLimited = null)
    {
        if (rateLimited == 1)
        {
            TempData["ErrorMsg"] = "Você está fazendo muitos scans de uma vez, respire !";
        }
        return View();
    }

    public async Task<IActionResult> AcessoRestrito()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ScanPolicy")]
    public async Task<IActionResult> Scan(string domainInput, HistoryModel historyModel)
    {
        try
        {
            if (NetworkUtils.IsIPAddress(domainInput))
            {
                TempData["ErrorMsg"] = "O endereço precisa ser uma URL, não um endereço IP";
                return View("Index");
            }
            else if (!NetworkUtils.IsValidUrl(domainInput, out Uri? uriResult))
            {
                TempData["ErrorMsg"] = "O endereço precisa ser válido, exemplo: www.google.com !";
                return View("Index");
            }
            else if (!await NetworkUtils.IsReachableAsync(domainInput))
            {
                TempData["ErrorMsg"] = "O endereço informado não está acessível. Verifique se está correto.";
                return View("Index");
            }

            var historyId = await _scanService.RunScanAndPersist(domainInput, historyModel, HttpContext.RequestAborted);

            TempData["OkMsg"] = "Scan feito com sucesso !";

            return RedirectToAction("Index", "History");
        }
        catch (TimeoutException)
        {
            TempData["ErrorMsg"] = "O scan demorou muito tempo e foi cancelado. Tente novamente.";
            return View("Index");
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMsg"] = ex.Message;
            return View("Index");
        }
        catch (OperationCanceledException)
        {
            TempData["ErrorMsg"] = "A operação foi cancelada pelo usuário.";
            return View("Index");
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Houve um erro ao fazer o scan !";
            return View("Index");
        }
    }

}
