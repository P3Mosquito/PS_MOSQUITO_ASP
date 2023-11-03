using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ps_mosquito_asp.Controllers
{
    //[Authorize]
    [Authorize(Roles = "Administrador,R" )]
    public class ReporteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
