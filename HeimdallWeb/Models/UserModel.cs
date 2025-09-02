using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("tb_user")]
    public class UserModel
    {
        [Key]
        public int user_id { get; set; }

        [Required]
        public required string username { get; set; }

        [Required]
        [EmailAddress]
        public required string email { get; set; }

        [Required]
        public required string password { get; set; }

        [Required]
        public required int user_type { get; set; } = (int)Enums.UserType.Default;

        [Required]
        public required DateTime created_at { get; set; } = DateTime.Now;

        public DateTime? updated_at { get; set; }

        public virtual List<HistoryModel> ?Histories { get; set; }
    }
}
