using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        #region Ctor
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitofwork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork _unitofwork)
        {
            this._logger = logger;
            this.unitofwork = _unitofwork;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> objProduct = await unitofwork.Product.GetAsync(null, false, x => x.Category);
          return View(objProduct);
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int productId)
        {

            ShoppingCart cart = new()
            {
                Product = await unitofwork.Product.GetFirstOrDefaultAsync(x => x.Id == productId, false, x => x.Category),
                Count = 1,
                ProductId = productId


            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
          var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart CartFromDb = await unitofwork.ShoppingCart.GetFirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.ProductId == shoppingCart.ProductId);

            if(CartFromDb!= null)
            {
                CartFromDb.Count += shoppingCart.Count;
                await unitofwork.ShoppingCart.CreateOrUpdateAsync(CartFromDb);
                await unitofwork.Save();


            }
            else
            {
                await unitofwork.ShoppingCart.CreateOrUpdateAsync(shoppingCart);
                await unitofwork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                   (await unitofwork.ShoppingCart.GetAsync(u => u.ApplicationUserId == userId)).Count());



            }
            TempData["Success"] = "Cart Updated Successfully";



            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion
    }
}
