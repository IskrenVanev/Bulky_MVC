using BulkyBook.Models;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Services
{
    public interface IUserService
    {
        RoleManagementVM? GetRoleManagementVm(string userId);
        bool UpdateRoleManagement(RoleManagementVM roleManagementVm);
        IEnumerable<ApplicationUser> GetAllUsersWithRoles();
        bool ToggleLockout(string userId);
    }
}
