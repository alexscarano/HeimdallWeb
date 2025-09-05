using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.DTO
{
    public class DeleteUserDTO
    {
        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public required string password { get; set; }

        [Required(ErrorMessage = "Confirme a senha")]
        [DataType(DataType.Password)]
        [Compare(nameof(password), ErrorMessage = "As senhas não coincidem")]
        public required string confirm_password { get; set; }

        [Required(ErrorMessage = "É necessário confirmar a exclusão")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Você precisa confirmar a exclusão")]
        public bool confirm_delete { get; set; }
    }
}
