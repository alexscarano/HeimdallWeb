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
            private readonly ITechnologyRepository _technologyRepository;
            public HistoryController(
                IHistoryRepository historyRepository, 
                IFindingRepository findingRepository,
                ITechnologyRepository technologyRepository
            )
            {
                _historyRepository = historyRepository;
                _findingRepository = findingRepository;
                _technologyRepository = technologyRepository;
            }

            [Authorize]
<<<<<<< Updated upstream
            public async Task<IActionResult> Index(int userId, int page = 1, int pageSize = 10)
=======
            public async Task<IActionResult> Index(int userId, string? where, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
>>>>>>> Stashed changes
            {
                int maxPageSize = 10;
                pageSize = Math.Min(pageSize, maxPageSize);
                page = Math.Max(page, 1);

<<<<<<< Updated upstream
                var histories = await _historyRepository.getHistoriesByUserID(userId, page, pageSize);
=======
                ViewData["CurrentSearch"] = where;
                ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

                var histories = await _historyRepository.getHistoriesByUserID(userId, where, startDate, endDate, page, pageSize);
>>>>>>> Stashed changes

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

        [Authorize]
        public async Task<IActionResult> GetTechnologies(int id)
        {
            try
            {
                var tecnologies = await _technologyRepository.getTechnologiesByHistoryId(id);

                if (tecnologies is null || tecnologies.Count == 0)
                    return Json(string.Empty);
            
                return Json(tecnologies);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
