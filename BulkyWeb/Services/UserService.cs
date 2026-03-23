using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public RoleManagementVM? GetRoleManagementVm(string userId)
        {
            ApplicationUser? applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company");
            if (applicationUser == null)
            {
                return null;
            }

            RoleManagementVM roleVm = new RoleManagementVM
            {
                ApplicationUser = applicationUser,
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            roleVm.ApplicationUser.Role = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();
            return roleVm;
        }

        public bool UpdateRoleManagement(RoleManagementVM roleManagementVm)
        {
            ApplicationUser? applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVm.ApplicationUser.Id);
            if (applicationUser == null)
            {
                return false;
            }

            string? oldRole = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();
            if (string.IsNullOrEmpty(oldRole))
            {
                return false;
            }

            if (roleManagementVm.ApplicationUser.Role != oldRole)
            {
                if (roleManagementVm.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVm.ApplicationUser.CompanyId;
                }

                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVm.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagementVm.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagementVm.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return true;
        }

        public IEnumerable<ApplicationUser> GetAllUsersWithRoles()
        {
            List<ApplicationUser> userList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();
            foreach (var user in userList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company { Name = "" };
                }
            }

            return userList;
        }

        public bool ToggleLockout(string userId)
        {
            ApplicationUser? userFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (userFromDb == null)
            {
                return false;
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _unitOfWork.ApplicationUser.Update(userFromDb);
            _unitOfWork.Save();
            return true;
        }
    }
}
