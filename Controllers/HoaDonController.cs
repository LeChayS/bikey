using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class HoaDonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChiTiet()
        {
            return View();
        }
    }
}
