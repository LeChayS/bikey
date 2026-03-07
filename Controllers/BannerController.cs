using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class BannerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
