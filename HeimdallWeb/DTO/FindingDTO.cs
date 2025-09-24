namespace HeimdallWeb.DTO
{
    public class FindingDTO
    {
        public string descricao { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string risco { get; set; } = string.Empty;
        public string evidencia { get; set; } = string.Empty;
        public string recomendacao { get; set; } = string.Empty;
    }
    public class FindingsWrapper
    {
        public string alvo { get; set; } = string.Empty;
        public string resumo { get; set; } = string.Empty;
        public List<FindingDTO> achados { get; set; }
    }
}
