using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HeimdallWeb.Helpers;

namespace HeimdallWeb.Models
{
    [Table("tb_user")]
    public class UserModel
    {
        /// <summary>
        /// A chave primária do usuário
        /// </summary>
        [Key]
        public int user_id { get; set; }

        /// <summary>
        /// O login/username do usuario
        /// </summary>
        [Required(ErrorMessage = "O campo usuário não pode estar vazio")]
        [MaxLength(30, ErrorMessage = "O usuário passou o limite máximo de caracteres")]
        [MinLength(6, ErrorMessage = "O usuário precisa ter no mínimo 6 caracteres")]
        public required string username { get; set; }

        [Required(ErrorMessage = "O campo email não pode estar vazio")]
        [EmailAddress(ErrorMessage = "O email deve ser válido")]
        public required string email { get; set; }

        [Required(ErrorMessage = "O campo senha não pode estar vazio")]
        [MaxLength(50, ErrorMessage = "A senha passou o limite máximo de caracteres")]
        [MinLength(8, ErrorMessage = "A senha precisa ter no mínimo 8 caracteres")]
        [DataType(DataType.Password)]
        [RequireSpecialCharacter]
        public required string password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "O campo confirmar senha não pode estar vazio")]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "As senhas precisam coincidir")]
        [RequireSpecialCharacter]
        public required string confirm_password { get; set; }

        public required int user_type { get; set; } = (int)Enums.UserType.Default;

        public required DateTime created_at { get; set; } = DateTime.Now;

        public DateTime? updated_at { get; set; }

        public virtual List<HistoryModel> ?Histories { get; set; }

        public string hashUserPassword()
        {
            return password = password.hashPassword();
        }
    }
}
