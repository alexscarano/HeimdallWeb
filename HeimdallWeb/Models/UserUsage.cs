using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeimdallWeb.Models
{
    [Table("UserUsage")]
    public class UserUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public int RequestsCount { get; set; } = 0;

        public int TokensUsed { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public UserModel? User { get; set; }  // Relacionamento com tabela User
    }
}
