using System.Text.Json.Nodes;
using HeimdallWeb.Models;

namespace HeimdallWeb.DTO
{
    public static class FindingDTOMapper
    {
        public static FindingModel ToModel(FindingDTO dto, int history_id_param)
        {
            return new FindingModel
            {
                type = dto.categoria,
                description = dto.descricao,
                severity = dto.risco switch
                {
                    "Baixo" => Enums.SeverityLevel.Low,
                    "Medio" => Enums.SeverityLevel.Medium,
                    "Alto" => Enums.SeverityLevel.High,
                    "Critico" => Enums.SeverityLevel.Critical,
                    _ => Enums.SeverityLevel.Low
                },
                evidence = dto.evidencia,
                recommendation = dto.recomendacao,
                history_id = history_id_param
            };
        }
    }
}
