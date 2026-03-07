using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class ThongKeBaoCaoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
