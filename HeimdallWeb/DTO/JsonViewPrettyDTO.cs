namespace HeimdallWeb.DTO
{
    /// <summary>
    /// DTO para a view estruturada de JSON do scan
    /// Apresenta os dados BRUTOS do scanner de forma organizada
    /// </summary>
    public record JsonViewPrettyDTO(
        string Alvo,
        string? Resumo,
        ScanResultSummaryDTO? DadosGerais,
        List<HeaderInfoDTO>? Headers,
        List<SecurityHeaderDTO>? HeadersSeguranca,
        List<CookieInfoDTO>? Cookies,
        List<PortInfoDTO>? Portas,
        List<PathInfoDTO>? CaminhosTestados,
        List<HttpRedirectInfoDTO>? HttpRedirects,
        SslInfoDTO? SSL,
        RobotsInfoDTO? Robots,
        string JsonCompleto
    );
}
