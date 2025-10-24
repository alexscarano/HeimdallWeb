namespace HeimdallWeb.DTO
{
    public record FindingDTO(
        string descricao, 
        string categoria, 
        string risco, 
        string evidencia, 
        string recomendacao
    );
}
