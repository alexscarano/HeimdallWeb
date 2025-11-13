using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_ia_summary")]
    public class IASummaryModel
    {
        [Key]
        public int ia_summary_id { get; set; }

        /// <summary>
        /// Concise analysis summary for dashboards/reports
        /// </summary>
        public string? summary_text { get; set; }

        /// <summary>
        /// Predominant category (e.g., SSL, Headers, Ports, General)
        /// </summary>
        public string? main_category { get; set; }

        /// <summary>
        /// Predominant risk level (Low, Medium, High, Critical)
        /// </summary>
        public string? overall_risk { get; set; }

        /// <summary>
        /// Total number of findings
        /// </summary>
        public int total_findings { get; set; }

        /// <summary>
        /// Number of critical findings
        /// </summary>
        public int findings_critical { get; set; }

        /// <summary>
        /// Number of high findings
        /// </summary>
        public int findings_high { get; set; }

        /// <summary>
        /// Number of medium findings
        /// </summary>
        public int findings_medium { get; set; }

        /// <summary>
        /// Number of low findings
        /// </summary>
        public int findings_low { get; set; }

        /// <summary>
        /// Additional AI suggestions or notes
        /// </summary>
        public string? ia_notes { get; set; }

        [Required]
        public DateTime created_date { get; set; } = DateTime.Now;

        public int? history_id { get; set; }

        [ForeignKey("history_id")]
        public HistoryModel? History { get; set; }
    }
}
