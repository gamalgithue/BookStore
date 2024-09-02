using Bulky.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Controllers
{
    public class CategoryController : Controller
    {

        #region prop
        private readonly ApplicationDbContext dbContext;
        #endregion


        #region Ctor
        public CategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        #endregion


        #region Actions
        public async Task<IActionResult> Index()
        {

            List<Category> objCategory = await dbContext.Categories.ToListAsync();

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
                await dbContext.Categories.AddAsync(obj);
                await dbContext.SaveChangesAsync();
                TempData["Success"] = "Category Created Successfully";
            }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null|| id == 0)
            {
                return NotFound();
            }
            Category obj = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
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
                dbContext.Categories.Update(obj);
                await dbContext.SaveChangesAsync();
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
            Category obj = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(Category obj)
        {

                dbContext.Categories.Remove(obj);
                await dbContext.SaveChangesAsync();
            TempData["Success"] = "Category Deleted Successfully";

            return RedirectToAction("Index");
        }

        #endregion


    }
}
