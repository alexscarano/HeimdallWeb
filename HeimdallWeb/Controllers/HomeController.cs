using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HeimdallWeb.Scanners;
using HeimdallWeb.Helpers;
using HeimdallWeb.Repository;
using HeimdallWeb.Models;
using HeimdallWeb.IA;

namespace HeimdallWeb.Controllers;

public class HomeController : Controller
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IConfiguration _config;

    public HomeController(IHistoryRepository historyRepository, IConfiguration config)
    {
        _historyRepository = historyRepository;
        _config = config;
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

        if (NetworkUtils.IsIPAddress(domainInput))
        {
            TempData["ErrorMsg"] = "Por favor, insira um nome de domínio válido, não um endereço IP.";
            return View("Index", "Home");
        }

        ScannerManager scanner = new();
        var result = await scanner.RunAllAsync(domainInput);
        var formattedResult = JsonPreprocessor.PreProcessScanResults(result.ToString());

        GeminiService geminiService = new(_config);
        string iaResponse = await geminiService.GenerateTextAsyncFindings(formattedResult.ToString());

        //historyModel.target = domainInput;
        //historyModel.raw_json_result = formattedResult.ToString();
        //await _historyRepository.insertHistory(historyModel);

        return Content(iaResponse, "application/json");
    }

}
