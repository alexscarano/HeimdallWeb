using HeimdallWeb.Models;

namespace HeimdallWeb.Services.Interfaces
{
    public interface IScanService
    {
        /// <summary>
        /// Roda scanners, chama a IA, persiste o histórico and findings em uma única unidade de trabalho.
        /// Retorna o ID do histórico criado.
        /// </summary>
        Task<int> RunScanAndPersistAsync(string domain, HistoryModel historyModel, CancellationToken cancellationToken = default);
    }
}
