using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_ia_summary")]
    public class IASummaryModel
    {
        [Key]
        public int ia_summary_id {  get; set; }
        
        // Categoria do erro filtrada por IA
        public string ?category { get; set; }
        
        // Erro especifico
        public string ?issue {  get; set; }

        // Recomendação para a melhora dele
        public string ?recommendation { get; set; }

        [Required]
        public required DateTime created_date { get; set; } = DateTime.Now;

        public int ?history_id { get; set; }

        public HistoryModel? History { get; set; }
    }
}
