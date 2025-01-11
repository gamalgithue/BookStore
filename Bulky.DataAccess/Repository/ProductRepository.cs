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
    public class ProductRepository:Repository<Product>,IProductRepository
    {
        private readonly ApplicationDbContext db;

        public ProductRepository(ApplicationDbContext db):base(db)
        {
            this.db = db;
        }

        public async Task Update(Product obj)
        {
            var objFromDb = await db.Products.FirstOrDefaultAsync(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price100 = obj.Price100;
                objFromDb.Description = obj.Description;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.Author = obj.Author;
                objFromDb.ProductImages = obj.ProductImages;
                //if (obj.ImageUrl != null)
                //{
                //    objFromDb.ImageUrl = obj.ImageUrl;
                //}
            }
        }
    }
}

