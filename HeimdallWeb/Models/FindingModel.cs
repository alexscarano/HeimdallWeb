using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HeimdallWeb.Enums;

namespace HeimdallWeb.Models
{
    [Table("tb_finding")]
    public class FindingModel
    {
        /// <summary>
        /// chave primária do achado
        /// </summary>
        [Key]
        public int finding_id { get; set; }

        /// <summary>
        ///  tipo do achado (ex: SSL Expired, Weak Cipher, Open Port)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// descrição detalhada
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// Low, Medium, High, Critical
        /// </summary>
        [Required]
        public SeverityLevel severity { get; set; }

        /// <summary>
        /// dados técnicos que provam o achado
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string evidence { get; set; } = string.Empty;

        /// <summary>
        /// hora que foi criado o registro
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// recomendações para mitigar o achado
        /// </summary>
        [MaxLength(255)]
        public string recommendation { get; set; } = string.Empty;
        /// <summary>
        /// chave estrangeira para o histórico
        /// </summary>
        public int ?history_id { get; set; }

        /// <summary>
        /// Campo extra só para retorno no JSON
        /// </summary>
        [NotMapped]
        public string severity_string { get; set; } = string.Empty;

        // relacionamento
        public virtual HistoryModel? History { get; set; }
    }
}
