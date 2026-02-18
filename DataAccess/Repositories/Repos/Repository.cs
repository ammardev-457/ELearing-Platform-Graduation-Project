using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ELProject.Shared.DTOs;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : class
    {
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _dbSet = context.Set<T>();
        }
        
        
        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<PagedResult<T>> GetAsync(
            Expression<Func<T, bool>>? filter, 
            int pageNumber, 
            int pageSize)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            
            if (filter != null)
            {
                query.Where(filter);    
            }

            var total = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>{TotalCount = total, Items = items};
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}