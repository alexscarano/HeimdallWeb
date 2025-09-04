using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class LoginController : Controller {
    private readonly IUserRepository _userRepository;

    public LoginController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Index(){

        return View();
    }

    [HttpPost]
    public IActionResult Enter(UserModel user){
        try
        {
            if (ModelState.IsValid)
            {
                UserModel userDB = _userRepository.getUserByEmailOrLogin(user.email) ?? throw new Exception ("Não foi possivel consultar");

                Console.WriteLine("Consultado");

                if (PasswordUtils.VerifyPassword(user.password, userDB.password))
                {
                    Console.WriteLine("Verificado");
                    return RedirectToAction("Index", "Home");
                }

            }
            return View("Index");
        }

         catch (System.Exception)
         {
            throw;
         } 
    }
}