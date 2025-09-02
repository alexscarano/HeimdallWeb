using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_technology")]
    public class TechnologyModel
    {
        [Key]
        public required int technology_id { get; set; }

        [Required]
        // nome da tecnologia detectada
        public required string technology_name { get; set; }

        // versão da tecnologia detectada
        public string ?version { get; set; }

        [Required]
        public required DateTime created_at { get; set; } = DateTime.Now;

        public int ?history_id { get; set; }

        public HistoryModel ?History { get; set; }
    }
}
