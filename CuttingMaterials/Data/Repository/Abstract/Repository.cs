using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext dbContext;

        public Repository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Add(TEntity entity)
        {
            dbContext.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            dbContext.Set<TEntity>().AddRange(entities);
        }

        public int Count()
        {
            return dbContext.Set<TEntity>().Count();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicte)
        {
            return dbContext.Set<TEntity>().Where(predicte);
        }

        public bool Exist(Expression<Func<TEntity, bool>> predicte)
        {
            return dbContext.Set<TEntity>().Any(predicte);
        }

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> predicte)
        {
            return await dbContext.Set<TEntity>().AnyAsync(predicte);
        }

        public TEntity FindLast(Expression<Func<TEntity, bool>> predicte)
        {
            return dbContext.Set<TEntity>().Where(predicte).LastOrDefault();
        }

        public TEntity FindFirst(Expression<Func<TEntity, bool>> predicte)
        {
            return dbContext.Set<TEntity>().Where(predicte).FirstOrDefault();
        }

        public TEntity Get(int id)
        {
            return dbContext.Set<TEntity>().Find(id);
        }

        public TEntity GetAsNoTracking(int id)
        {
            var obj = dbContext.Set<TEntity>().Find(id);
            dbContext.Entry(obj).State = EntityState.Detached;
            return obj;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return dbContext.Set<TEntity>().ToList();
        }

        public IEnumerable<TEntity> GetAllAsNoTracking()
        {
            return dbContext.Set<TEntity>().AsNoTracking().ToList();
        }

        public void Remove(TEntity entity)
        {
            dbContext.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            dbContext.Set<TEntity>().RemoveRange(entities);
        }

        public TEntity FirstOrDefault()
        {
            return dbContext.Set<TEntity>().FirstOrDefault();
        }
    }
}
