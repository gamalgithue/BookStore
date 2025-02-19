﻿using BulkyBook.Data;
using BulkyBook.DataAccess.Extend;
using BulkyBook.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ApplicationUserRepository:Repository<ApplicationUser>,IApplicationUserRepository
    {
        private readonly ApplicationDbContext db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }
      
    }
}
