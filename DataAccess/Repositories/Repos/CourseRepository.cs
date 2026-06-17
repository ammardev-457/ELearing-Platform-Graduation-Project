using System.Linq.Expressions;
using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using ELProject.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class CourseRepository : Repository<Course, int> , ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginationResult<Course>> GetAllCoursesAsync(int pageNumber, int pageSize)
        {
            var items = await _context.Courses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Category)
                .ToListAsync();

            return new PaginationResult<Course> { TotalCount = items.Count, Items = items };
        }

        public async Task<CourseResult?> GetCourseWithDataAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.User)
                .Include(c => c.Category)
                .Include(c => c.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return null;

            var result = new CourseResult
            {
                Id = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                LongDescription = course.LongDescription,
                Thumbnail = course.Thumbnail,
                CreatedDate = course.CreatedDate,
                Level = course.Level,
                Price = course.Price,
                InstructorId = course.UserId,
                InstructorName = course.User.Name,
                CategoryId = course.CategoryId,
                CategoryName = course.Category.Name,
                Sections = course.Sections.Select(s => new SectionResult
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    Lessons = s.Lessons.Select(l => new LessonResult
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Order = l.Order,
                        Type = l.Type,
                        DurationInSeconds = l.DurationInSeconds ?? 60
                    }).ToList()
                }).ToList()
            };

            return result;
        }

    }
}