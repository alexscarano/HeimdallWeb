using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HeimdallWeb.Models;

namespace HeimdallWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}
