using Microsoft.AspNetCore.Mvc;

namespace ps_mosquito_asp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        
    }
}
