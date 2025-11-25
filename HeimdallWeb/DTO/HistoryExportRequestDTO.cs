namespace HeimdallWeb.DTO
{
    /// <summary>
    /// DTO para solicitar exportação de histórico em PDF
    /// </summary>
    public class HistoryExportRequestDTO
    {
        /// <summary>
        /// ID do usuário para filtrar o histórico
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Página atual (para exportar registros específicos)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Quantidade de registros por página
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
