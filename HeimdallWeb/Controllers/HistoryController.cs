using ASHelpers.Extensions;
using HeimdallWeb.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Controllers
{
    public class HistoryController : Controller
    {
            private readonly IHistoryRepository _historyRepository;
            private readonly IFindingRepository _findingRepository;
            public HistoryController(IHistoryRepository historyRepository, IFindingRepository findingRepository)
            {
                _historyRepository = historyRepository;
                _findingRepository = findingRepository;
            }

            [Authorize]
            public async Task<IActionResult> Index(int userId, int page = 1, int pageSize = 10)
            {
                int maxPageSize = 10;
                pageSize = Math.Min(pageSize, maxPageSize);
                page = Math.Max(page, 1);

                var histories = await _historyRepository.getHistoriesByUserID(userId, page, pageSize);

                return View(histories); 
            }

        [HttpPost]
        [Authorize]
        public async Task<JObject> DeleteHistory(int id)
        {
            try
            {
                var historyDB = await _historyRepository.getHistoryById(id);
                if (historyDB is null)
                    return new { success = false, message = "Item no histórico não encontrado." }.ToJson();

                bool deleted = await _historyRepository.deleteHistory(id);
                if (deleted)
                    return new { success = true, message = "Item do histórico deletado com sucesso." }.ToJson();

                return new { success = false, message = "Falha ao deletar item do histórico." }.ToJson();
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Erro: " + ex.Message }.ToJson();
            }
        }

        [Authorize]
        public async Task<IActionResult> ViewJson(int id)
        {
            var jsonResult = await _historyRepository.getJsonByHistoryId(id);
            if (jsonResult is null)
            {
                TempData["ErrorMsg"] = "Falha ao carregar JSON do histórico.";
                return RedirectToAction("Index", "Home");
            }
            
            if (!jsonResult.HasValues)
            {
                return RedirectToAction("Index", "History");
            }

            return Content(jsonResult.ToString(), "application/json");
        }

        [Authorize]
        public async Task<IActionResult> GetFindings(int id)
        {
            try
            {
                var findings = await _findingRepository.getFindingsByHistoryId(id);

                if (findings is null || findings.Count == 0)
                    return Json(string.Empty);

                return Json(findings);
            }
            catch (Exception) 
            {
                return BadRequest();
            }
        }
    }
}
