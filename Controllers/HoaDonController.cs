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
        private readonly IDataChangeCheckService _dataChangeCheckService;

        public HoaDonController(IUserService userService, IHoaDonService hoaDonService, IDataChangeCheckService dataChangeCheckService)
            : base(userService)
        {
            _hoaDonService = hoaDonService;
            _dataChangeCheckService = dataChangeCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            DateTime? tuNgay,
            DateTime? denNgay,
            int page = 1,
            int pageSize = 10)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHoaDon);
            if (permissionCheck != null) return permissionCheck;

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

            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHoaDon);
            if (permissionCheck != null) return permissionCheck;

            var hoaDon = await _hoaDonService.GetByIdAsync(id);
            return hoaDon is null ? NotFound() : View(hoaDon);
        }

        [HttpGet]
        public async Task<IActionResult> InHoaDon(int id)
        {
            if (id <= 0) return NotFound();

            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHoaDon);
            if (permissionCheck != null) return permissionCheck;

            var hoaDon = await _hoaDonService.GetByIdAsync(id);
            if (hoaDon is null) return NotFound();

            ViewBag.AutoPrint = true;
            return View("ChiTiet", hoaDon);
        }

        /// <summary>
        /// API endpoint for auto-refresh feature - returns checksum of current invoice list
        /// </summary>
        [HttpPost]
        [Route("HoaDon/GetDataChecksum")]
        public async Task<IActionResult> GetDataChecksum([FromBody] DataChecksumRequest request)
        {
            var result = await RequirePermissionAsync(p => p.CanViewHoaDon);
            if (result != null) return result;

            try
            {
                var invoices = await _hoaDonService.GetAllAsync();
                var checksum = _dataChangeCheckService.GenerateChecksum(invoices);
                return Json(new { success = true, checksum = checksum });
            }
            catch (Exception)
            {
                return Json(new { success = false, checksum = Guid.NewGuid().ToString() });
            }
        }
    }
}
