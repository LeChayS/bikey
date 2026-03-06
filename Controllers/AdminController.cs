using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
