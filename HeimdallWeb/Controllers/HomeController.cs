using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
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
    private readonly ILogRepository _logRepository;

    public HomeController(
        IHistoryRepository historyRepository
        , IConfiguration config
        , IFindingRepository findingRepository
        , IScanService scanService
        , ILogRepository logRepository)
    {
        _historyRepository = historyRepository;
        _config = config;
        _findingRepository = findingRepository;
        _scanService = scanService;
        _logRepository = logRepository;
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
            if (string.IsNullOrEmpty(domainInput))
            {
                TempData["ErrorMsg"] = "O endereço está vazio, preencha ele antes de começar o scan";
                return View("Index");
            }
            
            string input = domainInput.Trim();
            bool wasTested = false;

            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                input = $"https://{input}"; 
            }

            bool isReachable = await NetworkUtils.IsReachableAsync(input);
            if (!isReachable)
            {
                // try fallback to http
                if (input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    input = $"http://{input.Substring("https://".Length)}";
                }
                else if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    input = $"http://{input}";
                }
            }
            else
            {
                wasTested = true;
            }

            domainInput = input;

            if (!NetworkUtils.IsValidUrl(domainInput, out Uri? uriResult) && !NetworkUtils.IsIPAddress(domainInput))
            {
                TempData["ErrorMsg"] = "O endereço precisa ser válido, exemplo: www.google.com !";
                return View("Index");
            }
            if (!wasTested)
            {
                if (!await NetworkUtils.IsReachableAsync(domainInput))
                {
                    TempData["ErrorMsg"] = "O endereço informado não está acessível. Verifique se está correto.";
                    return View("Index");
                }
            }

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.INIT_SCAN,
                message = "Iniciando processo de varredura",
                source = "HomeController",
                details = $"Target: {domainInput}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });

            var historyId = await _scanService.RunScanAndPersist(domainInput, historyModel, HttpContext.RequestAborted);

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.SCAN_COMPLETED,
                message = "Scan finalizado com sucesso",
                source = "HomeController",
                history_id = historyId,
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });

            TempData["OkMsg"] = "Scan feito com sucesso !";

            return RedirectToAction("Index", "History");
        }
        catch (TimeoutException ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "HomeController",
                details = $"Timeout: {ex.Message}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });
            TempData["ErrorMsg"] = "O scan demorou muito tempo e foi cancelado. Tente novamente.";
            return View("Index");
        }
        catch (ArgumentException ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "HomeController",
                details = $"ArgumentException: {ex.Message}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });
            TempData["ErrorMsg"] = ex.Message;
            return View("Index");
        }
        catch (OperationCanceledException ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.SCAN_ERROR,
                message = "Erro durante o processo de scan",
                source = "HomeController",
                details = $"Cancelado: {ex.Message}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });
            TempData["ErrorMsg"] = "A operação foi cancelada pelo usuário.";
            return View("Index");
        }
        catch (Exception ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.UNHANDLED_EXCEPTION,
                message = "Erro inesperado não tratado",
                source = "HomeController",
                details = ex.ToString(),
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(HttpContext)
            });
            // Prefer an inner exception message when available, otherwise use the exception message.
            var detailedMessage = ex.InnerException?.Message;
            if (string.IsNullOrWhiteSpace(detailedMessage))
            {
                detailedMessage = ex.Message;
            }

            // If there's still no useful message, fall back to the generic user-facing message.
            if (string.IsNullOrWhiteSpace(detailedMessage))
            {
                detailedMessage = "Houve um erro ao fazer o scan !";
            }

            TempData["ErrorMsg"] = detailedMessage;
            return View("Index");
        }
    }

}
