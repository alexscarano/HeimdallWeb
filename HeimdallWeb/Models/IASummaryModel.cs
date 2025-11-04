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
<<<<<<< Updated upstream
        public string ?summary_text { get; set; }
        /// <summary>
        ///  Categoria predominante (ex: SSL, Headers, Portas, Geral)
        /// </summary>
        public string ?main_category { get; set; }
=======
        public string? summary_text { get; set; }
        /// <summary>
        ///  Categoria predominante (ex: SSL, Headers, Portas, Geral)
        /// </summary>
        public string? main_category { get; set; }
>>>>>>> Stashed changes

        /// <summary>
        /// Nível de risco predominante (Baixo, Medio, Alto, Critico)
        /// Padrão: Baixo
        /// </summary>
        public string? overall_risk { get; set; } = "Baixo";

        /// <summary>
        ///  Com observações adicionais que o IA pode ter sugerido
        /// </summary>
<<<<<<< Updated upstream
        public string ?notes { get; set; }
=======
        public string? notes { get; set; }
>>>>>>> Stashed changes

        [Required]
        public required DateTime created_date { get; set; } = DateTime.Now;

<<<<<<< Updated upstream
        public int ?history_id { get; set; }
=======
        public int? history_id { get; set; }
>>>>>>> Stashed changes

        public HistoryModel? History { get; set; }
    }
}
