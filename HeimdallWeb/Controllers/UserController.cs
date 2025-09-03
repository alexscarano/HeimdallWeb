using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult DeleteUser()
    {
        return View();
    }

    public IActionResult History()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult RegisterAction(UserModel user)
    {
        try
        {
            //if (ModelState.IsValid)
            //{
                _userRepository.insertUser(user);
                TempData["OkMsg"] = "O usuário foi cadastrado com sucesso";
                //return RedirectToAction("Index", "Home");
                return View("Register", user);
            //}
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro no cadastro do usuário";
        }
        return View("Register", user);
    }

    public IActionResult AlterUser()
    {
        return View();
    }    
}