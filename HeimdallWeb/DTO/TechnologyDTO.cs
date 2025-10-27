using System.Text.Json.Serialization;
using HeimdallWeb.Helpers;

namespace HeimdallWeb.DTO
{
    public class TechnologyDTO
    {
        public string nome_tecnologia { get; init; } = null!;

        [JsonConverter(typeof(EmptyStringToNullConverter))]
        public string? versao { get; init; }
        public string categoria_tecnologia { get; init; } = null!;
        public string descricao_tecnologia { get; init; } = null!;
    }
}
