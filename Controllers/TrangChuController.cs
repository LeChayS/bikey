using bikey.Services;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly ITrangChuService _trangChuService;

        public TrangChuController(ITrangChuService trangChuService)
        {
            _trangChuService = trangChuService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _trangChuService.BuildTrangChuViewModelAsync());
        }

        public async Task<IActionResult> DanhSachXe()
        {
            return View(await _trangChuService.BuildTrangChuViewModelAsync());
        }

        public IActionResult GioiThieu()
        {
            return View();
        }

        public IActionResult ChiNhanh()
        {
            return View();
        }

        public IActionResult TinTuc()
        {
            return View();
        }

        public IActionResult LienHe()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietXe(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction(nameof(DanhSachXe));
            }

            var model = await _trangChuService.GetChiTietXeAsync(slug);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}
