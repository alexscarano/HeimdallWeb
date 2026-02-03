using System.ComponentModel.DataAnnotations;
using HeimdallWeb.Helpers;
namespace HeimdallWeb.DTO
{
    public class UpdateUserDTO
    {
        [Key]
        public int user_id { get; set; }

        public string? profile_image_path { get; set; }

        // [MaxLength(30, ErrorMessage = "O usuário passou o limite máximo de caracteres")]
        // [MinLength(6, ErrorMessage = "O usuário precisa ter no mínimo 6 caracteres")]
        public string? username { get; set; }

        // [EmailAddress(ErrorMessage = "O email deve ser válido")]
        public string? email { get; set; }

        // [MaxLength(50, ErrorMessage = "A senha passou o limite máximo de caracteres")]
        // [MinLength(8, ErrorMessage = "A senha precisa ter no mínimo 8 caracteres")]
        // [DataType(DataType.Password)]
        // [RequireSpecialCharacter]
        public string? password { get; set; }

        // [DataType(DataType.Password)]
        // [Compare("password", ErrorMessage = "As senhas precisam coincidir")]
        // [RequireSpecialCharacter]
        public string? confirm_password { get; set; }

        public DateTime? updated_at { get; set; }
    }
}
