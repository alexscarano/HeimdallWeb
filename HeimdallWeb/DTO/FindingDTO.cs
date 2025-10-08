namespace HeimdallWeb.DTO
{
    public record FindingDTO(
        string descricao, 
        string categoria, 
        string risco, 
        string evidencia, 
        string recomendacao
    );

    public class FindingsWrapper
    {
        public string alvo { get; set; } = string.Empty;
        public string resumo { get; set; } = string.Empty;
        public List<FindingDTO> achados { get; set; }
    }
}
