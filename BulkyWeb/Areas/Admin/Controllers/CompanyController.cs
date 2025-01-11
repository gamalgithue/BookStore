using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Adm)]
    public class CompanyController : Controller
    {


        #region prop
        private readonly IUnitOfWork unitOfWork;

        #endregion


        #region Ctor
        public CompanyController(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }
        #endregion


        #region Actions
        public async Task<IActionResult> Index()
        {

            IEnumerable<Company> objCompany = await unitOfWork.Company.GetAsync();

            return View(objCompany);
        }

        [HttpGet]
        public async Task<IActionResult> UpSert(int? id)
        {



            if (id == null || id == 0)
            {
                return View(new Company());

            }
            else
            {
                Company CompanyObj = await unitOfWork.Company.GetFirstOrDefaultAsync(x => x.Id == id);
                return View(CompanyObj);
            }


        }
        [HttpPost]
        public async Task<IActionResult> Upsert(Company obj)
        {

            bool isNewProduct = obj.Id == 0;
           
            if (ModelState.IsValid)
            {
                await unitOfWork.Company.CreateOrUpdateAsync(obj);
                await unitOfWork.Save();
                if (isNewProduct)
                {
                    TempData["Success"] = "Company Created Successfully";

                }
                else
                {
                    TempData["Success"] = "Company Updated Successfully";

                }
                return RedirectToAction("Index");

            }
            else
            {

                return View(obj);

            }


        }

        #endregion

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objCompanyList = await unitOfWork.Company.GetAsync();
            return Json(new { data = objCompanyList });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var CompanyToBeDeleted = await unitOfWork.Company.GetFirstOrDefaultAsync(u => u.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await unitOfWork.Company.DeleteAsync(CompanyToBeDeleted);
            await unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
