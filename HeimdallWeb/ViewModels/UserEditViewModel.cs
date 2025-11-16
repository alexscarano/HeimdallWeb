using HeimdallWeb.DTO;

namespace HeimdallWeb.ViewModels
{
    public class UserEditViewModel
    {
        public UpdateUserDTO UpdateUser { get; set; } = new UpdateUserDTO();
        public DeleteUserDTO DeleteUser { get; set; } = new DeleteUserDTO();

        public IFormFile? ProfileImage { get; set; }
    }
}