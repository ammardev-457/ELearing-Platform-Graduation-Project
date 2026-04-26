using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class LessonRepository : Repository<Lesson, int>, ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
