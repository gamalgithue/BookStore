using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public interface IRepository<TEntity> where TEntity:class
    {

        public Task<IEnumerable<TEntity>> GetAsync(
         Expression<Func<TEntity, bool>>? filter = null,
         bool noTrack = false,
         params Expression<Func<TEntity, object>>[] includeProperties);

        public Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? filter = null,
              bool noTrack = false,
              params Expression<Func<TEntity, object>>[] includeProperties);

        public Task CreateOrUpdateAsync(TEntity entity);
        public Task DeleteAsync(TEntity entity);
        public object[] GetKeyValues(TEntity entity);

    
}
}
