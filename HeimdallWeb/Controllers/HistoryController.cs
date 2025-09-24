using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Controllers
{
    public class HistoryController : Controller
    {
        private readonly IHistoryRepository _historyRepository;
        public HistoryController(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
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
                if (historyDB == null)
                    return JObject.FromObject(new { success = false, message = "Item no histórico não encontrado." });

                bool deleted = await _historyRepository.deleteHistory(id);
                if (deleted)
                    return JObject.FromObject(new { success = true, message = "Item do histórico deletado com sucesso." });

                return JObject.FromObject(new { success = false, message = "Falha ao deletar item do histórico." });
            }
            catch (Exception ex)
            {
                return JObject.FromObject(new { success = false, message = "Erro: " + ex.Message });
            }
        }


    }
}
