using bikey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace bikey.Controllers
{
    public class TrangChuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
