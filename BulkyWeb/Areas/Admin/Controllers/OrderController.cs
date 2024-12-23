using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            OrderVM ordVm = new()
            {
                OrderHeader = await unitOfWork.OrderHeader.GetFirstOrDefaultAsync(x => x.Id == orderId, false, x => x.ApplicationUser),
                OrderDetial = await unitOfWork.OrderDetail.GetAsync(x => x.OrderHeaderId == orderId, false, x => x.Product)

            };
            return View(ordVm);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrders = await unitOfWork.OrderHeader.GetAsync(null, false, x => x.ApplicationUser);
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
    }
}
