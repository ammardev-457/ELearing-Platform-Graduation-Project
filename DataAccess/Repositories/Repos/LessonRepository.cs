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

        public async Task<Lesson?> GetEnrolledLessonForEnrolledStudent(string studentId, int lessonId)
        {
            return await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .ThenInclude(c => c.Enrollments)
                .Select(l => new Lesson
                {
                    Id = l.Id,
                    Title = l.Title,
                    Order = l.Order,
                    Type = l.Type,
                    FileUrl = l.FileUrl,
                    DurationInSeconds = l.DurationInSeconds,
                    QuizId = l.QuizId,
                    SectionId = l.SectionId,
                    Section = new Section
                    {
                        Id = l.Section.Id,
                        Title = l.Section.Title,
                        CourseId = l.Section.CourseId,
                        Course = new Course
                        {
                            Id = l.Section.Course.Id,
                            Title = l.Section.Course.Title,
                            Enrollments = l.Section.Course.Enrollments
                                .Where(e => e.StudentId == studentId)
                                .ToList()
                        }
                    }
                }).FirstOrDefaultAsync(l => l.Id == lessonId);
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
    }
}
