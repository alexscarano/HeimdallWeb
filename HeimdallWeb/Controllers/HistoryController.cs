using ASHelpers.Extensions;
using HeimdallWeb.DTO;
using HeimdallWeb.DTO.Mappers;
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
            private readonly IPdfService _pdfService;
            
            public HistoryController(
                IHistoryRepository historyRepository, 
                IFindingRepository findingRepository,
                ITechnologyRepository technologyRepository,
                IPdfService pdfService
            )
            {
                _historyRepository = historyRepository;
                _findingRepository = findingRepository;
                _technologyRepository = technologyRepository;
                _pdfService = pdfService;
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
            // Busca o histórico completo
            var history = await _historyRepository.getHistoryById(id);
            if (history is null)
            {
                TempData["ErrorMsg"] = "Histórico não encontrado.";
                return RedirectToAction("Index", "History");
            }

            // Busca o JSON bruto do scanner
            var jsonResult = await _historyRepository.getJsonByHistoryId(id);
            if (jsonResult is null || !jsonResult.HasValues)
            {
                TempData["ErrorMsg"] = "JSON do scan não disponível.";
                return RedirectToAction("Index", "History");
            }

            // Converte para DTO de apresentação usando o mapper
            var prettyDTO = JsonViewMapper.ToPrettyDTO(jsonResult, history.target, history.summary);

            return View(prettyDTO);
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportPdf([FromBody] HistoryExportRequestDTO request)
        {
            try
            {
                // Validar entrada
                if (request.UserId <= 0)
                    return BadRequest(new { success = false, message = "ID de usuário inválido." });

                // Buscar histórico com os filtros aplicados
                var histories = await _historyRepository.getHistoriesByUserID(
                    request.UserId, 
                    request.Page, 
                    request.PageSize
                );

                if (histories == null || !histories.Items.Any())
                    return NotFound(new { success = false, message = "Nenhum histórico encontrado." });

                // Obter nome do usuário atual
                var userName = User.Identity?.Name ?? "Usuário desconhecido";

                // Gerar PDF
                var pdfBytes = _pdfService.GenerateHistoryPdf(histories, userName);

                // Retornar arquivo PDF
                var fileName = $"Historico_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao gerar PDF: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportSinglePdf(int historyId)
        {
            try
            {
                // Validar entrada
                if (historyId <= 0)
                    return BadRequest(new { success = false, message = "ID de histórico inválido." });

                // Buscar histórico específico com Findings e Technologies
                var history = await _historyRepository.getHistoryByIdWithIncludes(historyId);

                if (history == null)
                    return NotFound(new { success = false, message = "Histórico não encontrado." });

                // Obter nome do usuário atual
                var userName = User.Identity?.Name ?? "Usuário desconhecido";

                // Gerar PDF individual
                var pdfBytes = _pdfService.GenerateSingleHistoryPdf(history, userName);

                // Retornar arquivo PDF
                var fileName = $"Scan_{history.target.Replace("https://", "").Replace("http://", "").Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao gerar PDF: " + ex.Message });
            }
        }
    }
}
