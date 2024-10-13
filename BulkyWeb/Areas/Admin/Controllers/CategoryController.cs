using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Adm)]

    public class CategoryController : Controller
    {

        #region prop
        private readonly IUnitOfWork unitOfWork;

        #endregion


        #region Ctor
        public CategoryController( IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }
        #endregion


        #region Actions
        public async Task<IActionResult> Index()
        {

            IEnumerable<Category> objCategory = await unitOfWork.Category.GetAsync();

            return View(objCategory);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category obj)
        {

            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Can't Exactly Match The Name");
            }
            if (ModelState.IsValid)
            {
                await unitOfWork.Category.CreateOrUpdateAsync(obj);
                await unitOfWork.Save();
                TempData["Success"] = "Category Created Successfully";
            }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category obj = await unitOfWork.Category.GetFirstOrDefaultAsync(x => x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {

            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Can't Exactly Match The Name");
            }
            if (ModelState.IsValid)
            {
                await unitOfWork.Category.CreateOrUpdateAsync(obj);
                await unitOfWork.Save();

                TempData["Success"] = "Category Updated Successfully";

            }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category obj = await unitOfWork.Category.GetFirstOrDefaultAsync(x => x.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(Category obj)
        {

            await unitOfWork.Category.DeleteAsync(obj);

            await unitOfWork.Save();

            TempData["Success"] = "Category Deleted Successfully";

            return RedirectToAction("Index");
        }

        #endregion


    }
}
