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
                productVm.Product = (await unitOfWork.Product.GetFirstOrDefaultAsync(x => x.Id == id,false,x=>x.ProductImages));
                return View(productVm);
            }

          

        }
        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM obj,List<IFormFile> files)
        {

            bool isNewProduct = obj.Product.Id == 0;
           
            if (ModelState.IsValid)
            {


                if (obj.Product.Id == 0)
                {
                    await unitOfWork.Product.CreateOrUpdateAsync(obj.Product);
                }
                else
                {
                    await unitOfWork.Product.Update(obj.Product);
                }
                await unitOfWork.Save();
               

                string wwRootPath = webHostEnvironment.WebRootPath;

                if (files != null)
                {

                    foreach(IFormFile file in files)

                    {
                        string FileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"Images\Products\Product-" +obj.Product.Id;

                        string finalPath = Path.Combine(wwRootPath,productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }
                        using (var fileStream = new FileStream(Path.Combine(finalPath, FileName), FileMode.Create))
                          {
                       await file.CopyToAsync(fileStream);
                          }

                        ProductImage productImage = new ()
                        {
                            ImageUrl = @"\" + productPath + @"\" + FileName,
                            ProductId = obj.Product.Id

                        };

                        if (obj.Product.ProductImages == null)
                        {
                            obj.Product.ProductImages = new List<ProductImage>();
                        }

                        obj.Product.ProductImages.Add(productImage);

                       }

                    await unitOfWork.Product.Update(obj.Product);
                    await unitOfWork.Save();


                }
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

        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var imageToBeDeleted = await unitOfWork.ProductImage.GetFirstOrDefaultAsync(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                await unitOfWork.ProductImage.DeleteAsync(imageToBeDeleted);
                await unitOfWork.Save();

                TempData["Success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
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

            string productPath = @"Images\Products\Product-" + id;
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
