using BulkyBook.DataAccess.Extend;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        #region ctor
        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailSender emailSender;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender)
        {
            this.unitOfWork = unitOfWork;
            this.emailSender = emailSender;
        }
        #endregion

        #region index

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = await unitOfWork.ShoppingCart.GetAsync(x => x.ApplicationUserId == userId, false, x => x.Product),
                OrderHeader = new()
            };
            var productimages = await unitOfWork.ProductImage.GetAsync();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages =  productimages.Where(x => x.ProductId == cart.ProductId).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


            return View(ShoppingCartVM);

            
        }
        #endregion


        #region Summary
        public async Task<IActionResult> Summary()
        {
			var claimsIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = await unitOfWork.ShoppingCart.GetAsync(x => x.ApplicationUserId == userId, false, x => x.Product),
				OrderHeader = new()
			};
			ShoppingCartVM.OrderHeader.ApplicationUser = await unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == userId);
			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}


			return View(ShoppingCartVM);
		}

        [HttpPost]
        [ActionName("Summary")]
		public async Task<IActionResult> SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity?)User?.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM.ShoppingCartList = await unitOfWork.ShoppingCart.GetAsync(x => x.ApplicationUserId == userId, false, x => x.Product);
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;

			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


			ApplicationUser user = await unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == userId);
		
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            await unitOfWork.OrderHeader.CreateOrUpdateAsync(ShoppingCartVM.OrderHeader);
             await unitOfWork.Save();
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Count = cart.Count,
                    Price = cart.Price

                };
                await unitOfWork.OrderDetail.CreateOrUpdateAsync(orderDetail);
                await unitOfWork.Save();
            }

			if (user.CompanyId.GetValueOrDefault() == 0)
			{
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl=domain+$"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl=domain+$"customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    
                    
                    Mode = "payment",
                

                };
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                await unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                await unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);




            }
			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

        #endregion

        #region OrderConfirmation
        public async Task<ActionResult>  OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == id,false,x=> x.ApplicationUser);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    await unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    await unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    await unitOfWork.Save();
                }
                HttpContext.Session.Clear();

            }
           await emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
               $"<p>New Order Created - {orderHeader.Id}</p>");
            var shoppingCarts = await unitOfWork.ShoppingCart
               .GetAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            await unitOfWork.ShoppingCart.DeleteRangeAsync(shoppingCarts);
           await unitOfWork.Save();
            return View(id);
        }
        #endregion

        #region Calculate The Count
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
                HttpContext.Session.SetInt32(SD.SessionCart,
                 (await unitOfWork.ShoppingCart.GetAsync(u => u.ApplicationUserId == cartDb.ApplicationUserId)).Count() - 1);

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
            HttpContext.Session.SetInt32(SD.SessionCart,
                  (await unitOfWork.ShoppingCart.GetAsync(u => u.ApplicationUserId == cartDb.ApplicationUserId)).Count()-1);

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

		#endregion
	}
}

