using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Scan(string domainInput, HistoryModel historyModel)
    {
        #region Verificações de input

        if (NetworkUtils.IsIPAddress(domainInput))
        {
            TempData["ErrorMsg"] = "Por favor, insira um nome de domínio válido, não um endereço IP.";
            return View("Index", "Home");
        }

        #endregion

        try
        {
            var historyId = await _scanService.RunScanAndPersist(domainInput, historyModel);

            TempData["OkMsg"] = "Scan feito com sucesso !";

            return RedirectToAction("Index", "History");
        }
        catch (TimeoutException)
        {
            TempData["ErrorMsg"] = "O scan demorou muito tempo e foi cancelado. Tente novamente.";
            return View("Index", "Home");
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMsg"] = ex.Message;
            return View("Index", "Home");
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Houve um erro ao fazer o scan !";
            return View("Index");
        }
    }

}
