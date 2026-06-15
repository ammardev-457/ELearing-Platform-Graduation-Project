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
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<Lesson?> GetLessonWithInstructorId(string instructorId, int lessonId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .Where(l => l.Section.Course.UserId == instructorId)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }

        public async Task<int?> GetOrderOfLastLessonInSection(int sectionId)
        {
            return await _context.Lessons
            .Where(l => l.SectionId == sectionId)
            .MaxAsync(l => (int?)l.Order); // return null if there aren't lessons in the section
        }

    }
}
