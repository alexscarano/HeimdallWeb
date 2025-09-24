using System.Text.Json;
using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.IA;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using HeimdallWeb.Scanners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Controllers;

public class HomeController : Controller
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IFindingRepository _findingRepository;
    private readonly IConfiguration _config;

    public HomeController(
        IHistoryRepository historyRepository
        , IConfiguration config
        , IFindingRepository findingRepository)
    {
        _historyRepository = historyRepository;
        _config = config;
        _findingRepository = findingRepository;
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
        #region Verifica��es de input

        if (NetworkUtils.IsIPAddress(domainInput))
        {
            TempData["ErrorMsg"] = "Por favor, insira um nome de dom�nio v�lido, n�o um endere�o IP.";
            return View("Index", "Home");
        }

        #endregion

        try
        {

            #region Faz os scans e envia para a IA

            ScannerManager scanner = new();
            // realiza todos os scans com o ScannerManager 
            var result = await scanner.RunAllAsync(domainInput);
            // pr�-processa o JSON para facilitar a an�lise da IA
            var formattedResult = JsonPreprocessor.PreProcessScanResults(result.ToString());

            // envia para a IA
            GeminiService geminiService = new(_config);
            string iaResponse = await geminiService.GenerateTextAsyncFindings(formattedResult.ToString());
            // caso seja necess�rio extrair algo especifico do JSON
            using var doc = JsonDocument.Parse(iaResponse);
            
            #endregion

            #region Popula o modelo de hist�rico

            historyModel.target = domainInput;
            historyModel.raw_json_result = formattedResult.ToString();
            historyModel.summary = doc.RootElement.GetProperty("resumo").GetString();

            #endregion

            #region Insere tecnologias detectadas
            // inserir hist�rico no bd
            await _historyRepository.insertHistory(historyModel);

            // capturar chave prim�ria do hist�rico inserido para inserir a fk do finding
            int history_id = historyModel.history_id;
            // m�todo auxiliar que j� insere no banco, ele � necess�rio pois o IA pode gerar v�rios findings
            // e temos uma desserializa��o
            await _findingRepository.SaveFindingsFromIAAsync(iaResponse, history_id);

            #endregion

            TempData["OkMsg"] = "Scan feito com sucesso !";

            return RedirectToAction("Index", "History");
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Houve um erro ao fazer o scan !";
            return View("Index");
        }
    }

}
