using HeimdallWeb.Models.Map;
using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces
{
    public interface IPdfService
    {
        /// <summary>
        /// Gera PDF do histórico com base nos dados paginados
        /// </summary>
        /// <param name="histories">Resultado paginado do histórico</param>
        /// <param name="userName">Nome do usuário que está exportando</param>
        /// <returns>Array de bytes do PDF gerado</returns>
        byte[] GenerateHistoryPdf(PaginatedResult<HistoryModel> histories, string userName);

        /// <summary>
        /// Gera PDF detalhado de um site específico (histórico individual)
        /// </summary>
        /// <param name="history">Histórico específico do site</param>
        /// <param name="userName">Nome do usuário que está exportando</param>
        /// <returns>Array de bytes do PDF gerado</returns>
        byte[] GenerateSingleHistoryPdf(HistoryModel history, string userName);
    }
}
