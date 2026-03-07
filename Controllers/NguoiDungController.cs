using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class NguoiDungController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
