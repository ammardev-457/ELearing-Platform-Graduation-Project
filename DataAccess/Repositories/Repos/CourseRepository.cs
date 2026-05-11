using System.Linq.Expressions;
using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.DataAccess.Results;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
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

        public async Task<PaidCourseResult> GetPaidCourseWithDataAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(c => c.Id == id);

            var result = new PaidCourseResult
            {
                Id = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                LongDescription = course.LongDescription,
                Thumbnail = course.Thumbnail,
                CreatedDate = course.CreatedDate,
                Level = course.Level,
                Price = course.Price,
                CategoryId = course.CategoryId,
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
                        DurationInSeconds = l.DurationInSeconds ?? 60 // if file type is pdf, then duration is 60 seconds by default
                    }).ToList()
                }).ToList()
            };

            return result;
        }

        public async Task<Course> GetEnrolledCourseWithDataAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.Order))
                .Select(c => new Course
                {
                    Id = c.Id,
                    Title = c.Title,
                    Thumbnail = c.Thumbnail,
                    Level = c.Level,
                    CategoryId = c.CategoryId,
                    Sections = c.Sections.Select(s => new Section
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Order = s.Order,
                        Lessons = s.Lessons.Select(l => new Lesson
                        {
                            Id = l.Id,
                            Title = l.Title,
                            Order = l.Order,
                            Type = l.Type,
                            DurationInSeconds = l.DurationInSeconds ?? 60, // if file type is pdf, then duration is 60 seconds by default
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            return course;
        }
    }
}