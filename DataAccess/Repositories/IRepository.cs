using System.Linq.Expressions;
using ELProject.Shared.DTOs;

namespace ELProject.DataAccess.Repositories
{
    public interface IRepository<T, TKey> where T : class  
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<PagedResult<T>> GetAsync(Expression<Func<T, bool>>? filter, 
        int pageNumber, int pageSize);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

} 