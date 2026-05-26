using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
