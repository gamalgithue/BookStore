using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles =SD.Role_Adm)]
    public class OrderController : Controller
    {
        #region ctor
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        #endregion

        #region OrderDetails
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int orderId)
        {
           OrderVM = new()
            {
                OrderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == orderId, false, x => x.ApplicationUser),
                OrderDetial = await unitOfWork.OrderDetail.GetAsync(x => x.OrderHeaderId == orderId, false, x => x.Product)

            };
           

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Adm+","+SD.Role_Emp)]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderHeaderFroDb = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == OrderVM.OrderHeader.Id);
            orderHeaderFroDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFroDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFroDb.City = OrderVM.OrderHeader.City;
            orderHeaderFroDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFroDb.PostalCode= OrderVM.OrderHeader.PostalCode;
            orderHeaderFroDb.State = OrderVM.OrderHeader.State;
            

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFroDb.Carrier = OrderVM.OrderHeader.Carrier;


            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFroDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

            }
            await unitOfWork.OrderHeader.CreateOrUpdateAsync(orderHeaderFroDb);

            await unitOfWork.Save();
           

            TempData["Success"] = "OrderDeatils Updated Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFroDb.Id });


        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Adm + "," + SD.Role_Emp)]
        public async Task<IActionResult> StartProcessing()
        {
            await unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            await unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Adm + "," + SD.Role_Emp)]
        public async Task<IActionResult> ShipOrder()
        {

            var orderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x=> x.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            await unitOfWork.OrderHeader.CreateOrUpdateAsync(orderHeader);
            await unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Adm + "," + SD.Role_Emp)]
        public async Task<IActionResult> CancelOrder()
        {

            var orderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                await unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                await unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            await unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }

        [HttpPost]
        [ActionName("Details")]
        public async Task<IActionResult> Details_Pay_Now()
        {
            OrderVM.OrderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == OrderVM.OrderHeader.Id, false, x => x.ApplicationUser);
            OrderVM.OrderDetial = await unitOfWork.OrderDetail.GetAsync(x => x.OrderHeaderId == OrderVM.OrderHeader.Id, false, x => x.Product);
               var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),


                Mode = "payment",


            };
            foreach (var item in OrderVM.OrderDetial)
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
            await unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


        }
        public async Task<ActionResult> PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    await unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    await unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    await unitOfWork.Save();
                }

            }

            return View(orderHeaderId);


        }


        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
             await unitOfWork.OrderHeader.GetAsync(null, false, x => x.ApplicationUser);

            IEnumerable<OrderHeader> objOrders;


            if (User.IsInRole(SD.Role_Adm) || User.IsInRole(SD.Role_Emp))
            {
                objOrders = await unitOfWork.OrderHeader.GetAsync(null, false, x => x.ApplicationUser);

            }
            else
            {

                var claimsIdentity = (ClaimsIdentity?)User.Identity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                objOrders = await unitOfWork.OrderHeader
                    .GetAsync(x => x.ApplicationUserId == userId,false,x=>x.ApplicationUser);
            }
            switch (status)
            {
                case "pending":
                    objOrders = objOrders.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrders = objOrders.Where(x => x.OrderStatus == SD.StatusInProcess);

                    break;
                case "completed":
                    objOrders = objOrders.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrders = objOrders.Where(x => x.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = objOrders });
        }

        #endregion
    }
}
