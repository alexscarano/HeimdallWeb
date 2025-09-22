using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_history")]
    public class HistoryModel
    {
        [Key]
        public required int history_id { get; set; }

        [Required]
        /// <summary>
        /// Target escaneado (domínio)
        /// </summary>
        public required string target { get; set; }

        /// <summary>
        /// Resultado bruto do scan em formato JSON
        /// </summary>
        [Required]
        public required string raw_json_result { get; set; }

        [Required]
        public required DateTime created_date { get; set; } = DateTime.Now;

        public int ?user_id { get; set; }

        public UserModel ?User {  get; set; }

        public virtual List<TechnologyModel> ?Technologies { get; set; }

        public virtual List<IASummaryModel>? IASummaries { get; set; }

        public virtual List<FindingModel> ?Findings { get; set; }
    }
}
