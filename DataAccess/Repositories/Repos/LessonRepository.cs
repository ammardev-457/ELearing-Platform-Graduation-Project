using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class LessonRepository : Repository<Lesson, int>, ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetLessonsBySectionId(int sectionId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Where(l => l.SectionId == sectionId)
                .ToListAsync();
        }
    }
}
