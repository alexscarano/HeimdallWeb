using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_ia_summary")]
    public class IASummaryModel
    {
        [Key]
        public int ia_summary_id {  get; set; }

        /// <summary>
        /// Resumo geral da análise em poucas linhas (bom para dashboards/relatórios)
        /// </summary>
        public string ?summary_text { get; set; }
        /// <summary>
        ///  Categoria predominante (ex: SSL, Headers, Portas, Geral)
        /// </summary>
        public string ?main_category { get; set; }

        /// <summary>
        /// Nível de risco predominante (Baixo, Medio, Alto, Critico)
        /// Padrão: Baixo
        /// </summary>
        public string? overall_risk { get; set; } = "Baixo";

        /// <summary>
        ///  Com observações adicionais que o IA pode ter sugerido
        /// </summary>
        public string ?notes { get; set; }

        [Required]
        public required DateTime created_date { get; set; } = DateTime.Now;

        public int ?history_id { get; set; }

        public HistoryModel? History { get; set; }
    }
}
