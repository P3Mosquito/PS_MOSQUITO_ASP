using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using ps_mosquito_asp.Models;
using System.Diagnostics;

namespace ps_mosquito_asp.Controllers
{
    public class HomeController : Controller
    {
        //FirebaseAuthProvider auth;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBd0_dWTNOTQOA4vxbao9kWX6yEUWPhmuk"));
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}