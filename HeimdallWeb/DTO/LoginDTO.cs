using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "O campo Usuário/Email não pode estar vazio")]
        [MinLength(6, ErrorMessage = "O usuário precisa ter no mínimo 6 caracteres")]
        [MaxLength(30, ErrorMessage = "O usuário passou o limite máximo de caracteres")]
        public required string emailOrLogin { get; set; }

        [Required(ErrorMessage = "O campo senha não pode estar vazio")]
        [MaxLength(50, ErrorMessage = "A senha passou o limite máximo de caracteres")]
        [MinLength(8, ErrorMessage = "A senha precisa ter no mínimo 8 caracteres")]
        [DataType(DataType.Password)]
        public required string password { get; set; }
    }
}
