using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
   public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {

        public Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        public Task UpdateStripePaymentID(int id, string sessionId , string paymentIntentId );




    }
}
