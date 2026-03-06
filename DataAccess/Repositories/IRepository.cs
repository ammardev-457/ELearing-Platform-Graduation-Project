using System.Linq.Expressions;
using ELProject.Shared.DTOs;

namespace ELProject.DataAccess.Repositories
{
    public interface IRepository<T, TKey> where T : class  
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<PaginationResult<T>> GetAsync(Expression<Func<T, bool>>? filter, 
        int pageNumber = 1, int pageSize = 10);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

} 