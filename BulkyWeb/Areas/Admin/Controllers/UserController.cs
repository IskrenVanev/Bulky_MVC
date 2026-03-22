using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            
            return View("Index");
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM? roleVm = _userService.GetRoleManagementVm(userId);
            if (roleVm == null)
            {
                return NotFound();
            }

            return View(roleVm);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVm)
        {
            bool isUpdated = _userService.UpdateRoleManagement(roleManagementVm);
            if (!isUpdated)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<ApplicationUser> objUserList = _userService.GetAllUsersWithRoles();
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool isSuccess = _userService.ToggleLockout(id);
            if (!isSuccess)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            return Json(new { success = true, message = "Operation successful" });
        }
        
        #endregion
    }
}