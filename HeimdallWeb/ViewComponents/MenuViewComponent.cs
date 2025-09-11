using System.Security.Claims;
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

            var claimPrincipal = User as ClaimsPrincipal; 

            var roles = claimPrincipal?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // montando o objeto para passar para a view component
            var model = new MenuViewDTO
            {
                isAuthenticated = isAuthenticated,
                username = username,
                roles = roles
            };

            return View(model);
        }
    }
}
