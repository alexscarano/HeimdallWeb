using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.DTO
{
    public record LoginDTO(
        [Required(ErrorMessage = "O campo Usuário/Email não pode estar vazio")]
        [MinLength(6, ErrorMessage = "O usuário precisa ter no mínimo 6 caracteres")]
        [MaxLength(30, ErrorMessage = "O usuário passou o limite máximo de caracteres")]
        string emailOrLogin,
        [Required(ErrorMessage = "O campo senha não pode estar vazio")]
        [MaxLength(50, ErrorMessage = "A senha passou o limite máximo de caracteres")]
        [DataType(DataType.Password)]
        string password
    );

}
