using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
   public class OrderHeaderRepository : Repository<OrderHeader>,IOrderHeaderRepository
    {
        private readonly ApplicationDbContext db;

        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            this.db = db;
        }

        public async  Task Update(OrderHeader orderHeader)
        {
             db.OrderHeaders.Update(orderHeader);
            
        }

        public async Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var ordfrdb = await db.OrderHeaders.FirstOrDefaultAsync(x => x.Id == id);
            if (ordfrdb != null)
            {
                ordfrdb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    ordfrdb.PaymentStatus = paymentStatus;
                }
            }
        }

        public async Task UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var ordfrdb = await db.OrderHeaders.FirstOrDefaultAsync(x => x.Id == id);
            if (ordfrdb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    ordfrdb.SessionId = sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    ordfrdb.PaymentIntentId = paymentIntentId;
                    ordfrdb.PaymentDate = DateTime.Now;
                }

            }
        }
    }
}
