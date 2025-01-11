using BulkyBook.Data;
using BulkyBook.DataAccess.Extend;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Adm)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;


        #region prop

        #endregion


        #region Ctor
        public UserController(IUnitOfWork unitOfWork,RoleManager<IdentityRole> roleManager,UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        #endregion


        #region Actions
        public async Task<IActionResult> Index()
        {

           
            return View();
        }


        public async Task<IActionResult> RoleManagement(string userId)
        {
            // Retrieve the user with tracking enabled for updates
            var applicationUser = await unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(
                u => u.Id == userId,
                 false,// Enable tracking
                x => x.Company // Include navigation property
            );

            if (applicationUser == null)
            {
                return NotFound(); // Handle case where user is not found
            }

            // Retrieve the user's current role
            var roles = await userManager.GetRolesAsync(applicationUser);
            var currentRole = roles.FirstOrDefault();

            // Prepare the ViewModel
            var roleVM = new RoleManagementVM
            {
                ApplicationUser = applicationUser,
                RoleList = roleManager.Roles.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name
                }),
                CompanyList = (await unitOfWork.Company.GetAsync()).Select(company => new SelectListItem
                {
                    Text = company.Name,
                    Value = company.Id.ToString()
                })
            };

            // Assign the current role to the ViewModel
            roleVM.ApplicationUser.Role = currentRole;

            // Return the view with the ViewModel
            return View(roleVM);
        }


        [HttpPost]
        public async Task<IActionResult> RoleManagement(RoleManagementVM roleManagementVM)
        {

            var applicationUser = await unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == roleManagementVM.ApplicationUser.Id, true);
        
            if (applicationUser == null)
            {
                return NotFound();
            }
                // Retrieve the current role
                var oldRole = (await userManager.GetRolesAsync(applicationUser)).FirstOrDefault();

            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                // Role was updated
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Comp)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                else if (oldRole == SD.Role_Comp)
                {
                    applicationUser.CompanyId = null;
                }

                // Update the user
                await unitOfWork.ApplicationUser.CreateOrUpdateAsync(applicationUser);
                await unitOfWork.Save();

                if (!string.IsNullOrEmpty(oldRole))
                {
                    await userManager.RemoveFromRoleAsync(applicationUser, oldRole);
                }

                await userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role);
            }
            else if (oldRole == SD.Role_Comp && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
            {
                // Only update CompanyId if it changed
                applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                await unitOfWork.ApplicationUser.CreateOrUpdateAsync(applicationUser);
                await unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }




        #endregion

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var UserList = await unitOfWork.ApplicationUser.GetAsync(null,false,x=>x.Company);

            


            foreach (var user in UserList)
            {
                user.Role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = UserList });
        }


        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {

            var objFromDb = await unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            await unitOfWork.ApplicationUser.CreateOrUpdateAsync(objFromDb);
            await unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
