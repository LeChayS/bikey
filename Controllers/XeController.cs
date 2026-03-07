using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class XeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
