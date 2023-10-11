using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ps_mosquito_asp.Models;

namespace ps_mosquito_asp.Controllers
{
    public class LoginController : Controller
    {
        
        
        public IActionResult Index()
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
        public IActionResult Logout()
        {
            return View();
        }
       

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnURL)
        {
            //try
            //{
            //    if (Request.IsAuthenticated)
            //    {
            //        return RedirectToRoute(returnURL);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.Write(ex.Message);
            //}
            return View();
        }
    }
}
