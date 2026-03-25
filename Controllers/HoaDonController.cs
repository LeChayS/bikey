using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class HoaDonController : BaseController
    {
        private readonly IHoaDonService _hoaDonService;

        public HoaDonController(IUserService userService, IHoaDonService hoaDonService)
            : base(userService)
        {
            _hoaDonService = hoaDonService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            DateTime? tuNgay,
            DateTime? denNgay,
            int page = 1,
            int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _hoaDonService.GetPaginatedAsync(page, pageSize, searchString, tuNgay, denNgay);
            var todayCount = await _hoaDonService.GetCountTodayAsync();
            var revenue = result.Items.Sum(h => h.SoTien);

            var viewModel = new HoaDonIndexViewModel
            {
                HoaDonList = result.Items,
                TotalItems = result.Total,
                PageSize = pageSize,
                CurrentPage = page,
                SearchString = searchString,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                TongDoanhThu = revenue,
                SoHoaDonHomNay = todayCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            if (id <= 0) return NotFound();

            var hoaDon = await _hoaDonService.GetByIdAsync(id);
            return hoaDon is null ? NotFound() : View(hoaDon);
        }

        [HttpGet]
        public async Task<IActionResult> InHoaDon(int id)
        {
            if (id <= 0) return NotFound();

            var hoaDon = await _hoaDonService.GetByIdAsync(id);
            if (hoaDon is null) return NotFound();

            ViewBag.AutoPrint = true;
            return View("ChiTiet", hoaDon);
        }
    }
}
