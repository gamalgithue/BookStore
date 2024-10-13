using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = await unitOfWork.ShoppingCart.GetAsync(x => x.ApplicationUserId == userId, false, x => x.Product)
            };
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }


            return View(ShoppingCartVM);

            
        }

        public IActionResult Summary()
        {
            return View();
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cartDb = await unitOfWork.ShoppingCart.GetFirstOrDefaultAsync(x => x.Id == cartId);
            cartDb.Count += 1;
           await unitOfWork.ShoppingCart.CreateOrUpdateAsync(cartDb);
            await unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var cartDb = await unitOfWork.ShoppingCart.GetFirstOrDefaultAsync(x => x.Id == cartId);
            if (cartDb.Count <= 1)
            {
                await unitOfWork.ShoppingCart.DeleteAsync(cartDb);

            }
            else
            {
                cartDb.Count -= 1;
                await unitOfWork.ShoppingCart.CreateOrUpdateAsync(cartDb);
            }
            await unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var cartDb = await unitOfWork.ShoppingCart.GetFirstOrDefaultAsync(x => x.Id == cartId);
            await unitOfWork.ShoppingCart.DeleteAsync(cartDb);
            await unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;

                }
            }
        }
    }
}

