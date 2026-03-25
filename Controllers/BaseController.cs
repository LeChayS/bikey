using Microsoft.AspNetCore.Mvc;
using bikey.Services;
using bikey.Models;

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

        /// <summary>
        /// Gets the current user ID from claims.
        /// </summary>
        /// <returns>User ID if found, null otherwise</returns>
        protected int? GetCurrentUserId()
        {
            return _userService.GetUserIdFromClaims(User);
        }

        /// <summary>
        /// Requires a specific permission before allowing the action to proceed.
        /// </summary>
        /// <param name="permissionCheck">Lambda expression to check permission</param>
        /// <param name="redirectUrl">URL to redirect to if permission denied (defaults to AccessDenied page)</param>
        /// <returns>IActionResult if permission denied, null if allowed</returns>
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

            return null; // Permission granted
        }

        /// <summary>
        /// Synchronous version of permission check (for inline validation).
        /// </summary>
        /// <param name="permission">Permission object to check</param>
        /// <param name="permissionCheck">Lambda expression to check permission</param>
        /// <param name="redirectUrl">URL to redirect to if permission denied</param>
        /// <returns>IActionResult if permission denied, null if allowed</returns>
        protected IActionResult? RequirePermission(
            PhanQuyen? permission,
            Func<PhanQuyen, bool> permissionCheck,
            string redirectUrl = "/AccessDenied")
        {
            if (permission == null || !permissionCheck(permission))
            {
                return Redirect(redirectUrl);
            }

            return null; // Permission granted
        }
    }
}
