using Microsoft.AspNetCore.Mvc;
using bikey.Services;
using bikey.Models;
using System.Text.Json.Serialization;

namespace bikey.Controllers
{
    /// <summary>
    /// Base controller class that provides common functionality for all controllers.
    /// Includes permission checking and user ID retrieval methods.
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly IUserService _userService;

        public BaseController(IUserService userService)
        {
            _userService = userService;
        }

        protected int? GetCurrentUserId()
        {
            return _userService.GetUserIdFromClaims(User);
        }

        protected async Task<IActionResult?> RequirePermissionAsync(
            Func<PhanQuyen, bool> permissionCheck,
            string redirectUrl = "/AccessDenied")
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }
            var permission = await _userService.GetPermissionAsync(userId.Value);
            if (permission == null || !permissionCheck(permission))
            {
                return Redirect(redirectUrl);
            }
            return null;
        }

        protected IActionResult? RequirePermission(
            PhanQuyen? permission,
            Func<PhanQuyen, bool> permissionCheck,
            string redirectUrl = "/AccessDenied")
        {
            if (permission == null || !permissionCheck(permission))
            {
                return Redirect(redirectUrl);
            }
            return null;
        }

        /// <summary>
        /// API endpoint to get data checksum for auto-refresh detection
        /// Override this method in child controllers to implement specific data checksum logic
        /// </summary>
        [HttpPost]
        [Route("{controller}/GetDataChecksum")]
        public virtual async Task<IActionResult> GetDataChecksum([FromBody] DataChecksumRequest request)
        {
            // Default implementation - should be overridden in child controllers
            return Json(new { checksum = Guid.NewGuid().ToString() });
        }
    }

    public class DataChecksumRequest
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }
}

