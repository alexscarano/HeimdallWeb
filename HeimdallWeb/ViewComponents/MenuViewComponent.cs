using HeimdallWeb.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var username = isAuthenticated ? User.Identity?.Name : null;

            // montando o objeto para passar para a view component
            var model = new MenuViewDTO
            {
                isAuthenticated = isAuthenticated,
                username = username
            };

            return View(model);
        }
    }
}
