using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext db;

        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }


        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
		public IOrderHeaderRepository OrderHeader { get; private set; }
        
        public IProductImageRepository ProductImage { get; private set; }

        public UnitOfWork(ApplicationDbContext _db)
        {
            this.db = _db;
            Category = new CategoryRepository(db);
            Product = new ProductRepository(db);
            Company = new CompanyRepository(db);
            ShoppingCart = new ShoppingCartRepository(db);
            ApplicationUser = new ApplicationUserRepository(db);
            OrderDetail = new OrderDetailRepository(db);
            OrderHeader = new OrderHeaderRepository(db);
            ProductImage = new ProductImageRepository(db);
           


        }

        public async Task Save()
        {
           await  db.SaveChangesAsync();
        }
    }
}
