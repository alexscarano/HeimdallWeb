using HeimdallWeb.Models;

namespace HeimdallWeb.DTO.Mappers
{
    public static class TechnologyDTOMapper
    {
        public static TechnologyModel ToModel(TechnologyDTO dto, int history_id_param)
        {
            return new TechnologyModel
            {
                technology_name = dto.nome_tecnologia,
                version = dto.versao,
                technology_category = dto.categoria_tecnologia,
                technology_description = dto.descricao_tecnologia,
                history_id = history_id_param
            };
        }
    }
}
