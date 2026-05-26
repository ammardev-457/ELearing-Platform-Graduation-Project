using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                    .AsNoTracking()
                    .ToListAsync();
        }
    }
}
