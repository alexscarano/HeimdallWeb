using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_technology")]
    public class TechnologyModel
    {
        [Key]
        public int technology_id { get; set; }

        [Required]
        [MaxLength(100)]
        // nome da tecnologia detectada
        public required string technology_name { get; set; }

        // versão da tecnologia detectada
        [MaxLength(30)]
        public string ?version { get; set; }

        [Required]
        [MaxLength(50)]
        // categoria da tecnologia (ex: Web Server, CMS, Framework)
        public required string technology_category { get; set; }

        [Required]
        [MaxLength(1000)]
        // descrição da tecnologia (simples resumo e fraquezas)
        public required string technology_description { get; set; }

        [Required]
        public DateTime created_at { get; set; } = DateTime.Now;

        public int ?history_id { get; set; }

        public HistoryModel ?History { get; set; }
    }
}
