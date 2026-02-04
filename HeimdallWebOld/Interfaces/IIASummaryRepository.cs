using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces
{
    public interface IIASummaryRepository
    {
        /// <summary>
        /// Cria um resumo de IA baseado nos findings de um history_id
        /// </summary>
        Task SaveIASummaryFromFindings(int historyId, string iaResponse);
        
        /// <summary>
        /// Retorna resumos de IA para um usuário específico
        /// </summary>
        Task<List<IASummaryModel>> GetUserIASummaries(int userId, int limit = 10);
    }
}
