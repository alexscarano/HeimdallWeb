namespace HeimdallWeb.DTO
{
    public record AIResponseDTO(
       string alvo,
       string resumo,
       List<FindingDTO>? achados,
       List<TechnologyDTO>? tecnologias 
    );
}
