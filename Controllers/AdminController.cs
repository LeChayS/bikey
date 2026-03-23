using bikey.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
//using bikey.Repository;
//using bikey.ViewModels;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using bikey.Attributes;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminController : Controller
    {
        private readonly BikeyDbContext _context;
        public AdminController(BikeyDbContext context)
        {
            _context = context;
        }
        // GET: Admin
        //[PermissionAuthorize("CanViewBaoCao")]
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            return View();
        }

    }
}