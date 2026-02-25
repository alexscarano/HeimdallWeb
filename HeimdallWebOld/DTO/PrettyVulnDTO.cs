namespace HeimdallWeb.DTO
{
    /// <summary>
    /// Resumo geral do scan
    /// </summary>
    public record ScanResultSummaryDTO(
        int? StatusCode,
        string? Servidor,
        bool? RedirecionamentoHTTPS,
        int? TotalPortasAbertas,
        int? TotalCaminhosSensiveis,
        int? TotalCookies
    );

    /// <summary>
    /// Header HTTP individual
    /// </summary>
    public record HeaderInfoDTO(
        string Nome,
        string Valor
    );

    /// <summary>
    /// Security Header individual
    /// </summary>
    public record SecurityHeaderDTO(
        string Nome,
        string Status,
        string? Valor
    );

    /// <summary>
    /// Informação de porta aberta
    /// </summary>
    public record PortInfoDTO(
        string IP,
        int Porta,
        string Estado,
        string? Servico,
        string Severidade,
        string Descricao
    );

    /// <summary>
    /// Caminho sensível testado
    /// </summary>
    public record PathInfoDTO(
        string Caminho,
        int StatusCode,
        string Resultado,
        string? Evidencia
    );

    /// <summary>
    /// Informações SSL/TLS
    /// </summary>
    public record SslInfoDTO(
        bool Valido,
        string? Emissor,
        DateTime? DataExpiracao,
        int? DiasRestantes,
        string? VersaoProtocolo
    );

    /// <summary>
    /// Informações do robots.txt
    /// </summary>
    public record RobotsInfoDTO(
        bool Encontrado,
        bool SitemapEncontrado,
        string? SitemapUrl,
        List<RobotsAlertDTO>? Alertas,
        string? Conteudo
    );

    /// <summary>
    /// Alerta do robots.txt scanner
    /// </summary>
    public record RobotsAlertDTO(
        string Severidade,
        string Mensagem
    );

    /// <summary>
    /// Informação de cookie detectado
    /// </summary>
    public record CookieInfoDTO(
        string Nome,
        bool TemSecure,
        bool TemHttpOnly,
        string SameSite,
        string Risco,
        string Descricao
    );

    /// <summary>
    /// Informação de redirect HTTP
    /// </summary>
    public record HttpRedirectInfoDTO(
        string IP,
        bool PortaAberta,
        bool RedirectDetectado,
        string? RedirectUrl,
        string? Severidade,
        string? Descricao
    );
}
