using BulkyBook.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Adm)]
    public class ProductController : Controller
    {

        #region prop
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        #endregion


        #region Ctor
        public ProductController(IUnitOfWork _unitOfWork,IWebHostEnvironment _webHostEnvironment)
        {
            this.unitOfWork = _unitOfWork;
            this.webHostEnvironment = _webHostEnvironment;
        }
        #endregion


        #region Actions
        public async Task<IActionResult> Index()
        {

            IEnumerable<Product> objProduct = await unitOfWork.Product.GetAsync(null,false,x=>x.Category);
                
          
        

            return View(objProduct);
        }

        [HttpGet]
        public async Task<IActionResult> UpSert(int? id)
        {

            ProductVM productVm = new ProductVM()
            {
                CategoryList = (await unitOfWork.Category.GetAsync()).Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                }),
                Product = new Product()

            };

            if(id==null|| id == 0)
            {
                return View(productVm);

            }
            else
            {
                productVm.Product = (await unitOfWork.Product.GetFirstOrDefaultAsync(x => x.Id == id));
                return View(productVm);
            }

            //  IEnumerable<SelectListItem> CategoryList = (await unitOfWork.Category.GetAsync()).Select(u => new SelectListItem
            //{
            //    Value = u.Id.ToString(), // Assuming Id is the unique identifier
            //    Text = u.Name, // Assuming Name is the display text
            //                   // If applicable
            //});
            //ViewData["CategoryList"] = CategoryList;

        }
        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM obj,IFormFile? file)
        {

            bool isNewProduct = obj.Product.Id == 0;
            //if (obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "The Display Order Can't Exactly Match The Name");
            //}
            if (ModelState.IsValid)
            {

                string wwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwRootPath, @"Images\Product");

                    if (!string.IsNullOrEmpty(obj.Product.ImgaeUrl))
                    {
                        var oldImage = Path.Combine(wwRootPath, obj.Product.ImgaeUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, FileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    obj.Product.ImgaeUrl = @"\Images\Product\" + FileName;

                }
                await unitOfWork.Product.CreateOrUpdateAsync(obj.Product);
                await unitOfWork.Save();
               if (isNewProduct)
                {
                    TempData["Success"] = "Product Created Successfully";

                }
                else
                {
                    TempData["Success"] = "Product Updated Successfully";

                }
                return RedirectToAction("Index");

            }
            else
            {
                ProductVM productVm = new ProductVM()
                {
                    CategoryList = (await unitOfWork.Category.GetAsync()).Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.Name,
                    }),
                    Product = new Product()

                };
                return View();

            }


        }

       

        #endregion

        #region API Calls

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Product> objProduct = await unitOfWork.Product.GetAsync(null, false, x => x.Category);

            return Json(new { data = objProduct });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var productToBeDeleted = await unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            await unitOfWork.Product.DeleteAsync(productToBeDeleted);
            await unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion


    }
}
