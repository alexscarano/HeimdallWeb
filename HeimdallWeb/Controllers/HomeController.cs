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
        #region Verificações de input

        if (NetworkUtils.IsIPAddress(domainInput))
        {
            TempData["ErrorMsg"] = "Por favor, insira um nome de domínio válido, não um endereço IP.";
            return View("Index", "Home");
        }

        #endregion

        try
        {

            #region Faz os scans e envia para a IA

            ScannerManager scanner = new();
            // realiza todos os scans com o ScannerManager 
            var scanTask = scanner.RunAllAsync(domainInput);

            #region Configurando timeout para o scan

            var timeOutTask = Task.Delay(TimeSpan.FromMinutes(1)); // 60 segundos de timeout
            var completedTask = await Task.WhenAny(scanTask, timeOutTask); // espera qualquer uma das tasks estar completas

            if (completedTask == timeOutTask)
            {
                TempData["ErrorMsg"] = "O scan demorou muito tempo e foi cancelado. Tente novamente.";
                return View("Index", "Home");
            }

            var result = await scanTask;
            string jsonString = result.ToString();

            #endregion

            // pr�-processa o JSON para facilitar a an�lise da IA
            JsonPreprocessor.PreProcessScanResults(ref jsonString);

            // envia para a IA
            GeminiService geminiService = new(_config);
            string iaResponse = await geminiService.GenerateTextAsyncFindings(jsonString);
            // caso seja necess�rio extrair algo especifico do JSON
            using var doc = JsonDocument.Parse(iaResponse);
            
            #endregion

            #region Popula o modelo de histórico

            historyModel.target = domainInput;
            historyModel.raw_json_result = jsonString;
            historyModel.summary = doc.RootElement.GetProperty("resumo").GetString();

            #endregion

            #region Insere tecnologias detectadas
            // inserir histórico no bd
            await _historyRepository.insertHistory(historyModel);

            // capturar chave prim�ria do hist�rico inserido para inserir a fk do finding
            int history_id = historyModel.history_id;
            // m�todo auxiliar que j� insere no banco, ele é necess�rio pois o IA pode gerar v�rios findings
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
