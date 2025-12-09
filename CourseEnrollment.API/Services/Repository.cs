using CourseEnrollment.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CourseEnrollment.API.Services
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext context;
        private readonly DbSet<T> dbSet;

        public Repository(AppDbContext context)
        {
            this.context = context;
            dbSet = this.context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<List<T>> GetListAsync()
        {
            return await dbSet.ToListAsync();
        }
        public async Task<List<T>> GetListAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.Where(predicate).ToListAsync();
        }
        public async Task UpdateAsync(T entity)
        {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task<List<T>> AddListAsync(List<T> entity)
        {
            dbSet.AddRange(entity);
            await context.SaveChangesAsync();
            return entity;
        }
    }
        public interface IRepository<T>
        {
            public Task<T> AddAsync(T entity);
            public Task<List<T>> AddListAsync(List<T> entity);
            public Task<T> GetByIdAsync(Guid id);
            public Task DeleteAsync(Guid id);
            public Task UpdateAsync(T entity);
            public Task<List<T>> GetListAsync();
            public Task<List<T>> GetListAsync(params Expression<Func<T, object>>[] includes);
            Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
            Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);
        }
    
}
