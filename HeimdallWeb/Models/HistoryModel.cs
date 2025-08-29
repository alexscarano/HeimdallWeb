using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.Models
{
    public class HistoryModel
    {
        [Key]
        public required int history_id { get; set; }

        [Required]
        // Url ou IP do website
        public required string target { get; set; }

        // Resumo de alertas críticos
        public string ?summary { get; set; }

        // resultado bruto
        public string ?raw_json_result { get; set; }

        [Required]
        public required DateTime created_date { get; set; } = DateTime.Now;

        public int ?user_id { get; set; }

        public UserModel ?User {  get; set; }

        public virtual List<TechnologyModel> ?Technologies { get; set; }

        public virtual List<IASummaryModel>? IASummaries { get; set; }
    }
}
