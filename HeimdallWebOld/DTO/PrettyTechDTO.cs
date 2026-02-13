namespace HeimdallWeb.DTO
{
    /// <summary>
    /// DTO para apresentação de tecnologias na view
    /// Usa nomes legíveis e estrutura simplificada
    /// </summary>
    public record PrettyTechDTO(
        string Nome,
        string? Versao,
        string Categoria,
        string Descricao
    );
}
