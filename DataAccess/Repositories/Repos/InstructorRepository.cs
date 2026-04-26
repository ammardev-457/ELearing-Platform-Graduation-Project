using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Instructor;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class InstructorRepository : Repository<Course, int>, IInstructorRepository
    {
        private readonly AppDbContext _context;

        public InstructorRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InstructorCourseDto>> GetInstructorCoursesAsync(string instructorId)
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.UserId == instructorId)
                .Select(c => new InstructorCourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedDate = c.CreatedDate,
                    Rate = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0,
                    StudentsCount = c.Enrollments.Count(),
                    Revenue = c.Orders.Where(o => o.Status == OrderStatus.Success.ToString()).Sum(o => (decimal?)o.Amount) ?? 0
                })
                .ToListAsync();

            return courses;
        }

        public async Task<InstructorDashboardDto> GetInstructorDashboardAsync(string instructorId)
        {
            var coursesQuery = _context.Courses.Where(c => c.UserId == instructorId);

            var coursesCount = await coursesQuery.CountAsync();

            var totalStudents = await _context.Enrollments
                .Where(e => e.Course.UserId == instructorId)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var avgRating = await _context.Reviews
                .Where(r => r.Course.UserId == instructorId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0;

            var totalRevenue = await _context.Orders
                .Where(o => o.Course.UserId == instructorId && o.Status == OrderStatus.Success.ToString())
                .Select(o => (decimal?)o.Amount)
                .SumAsync() ?? 0;

            var courses = await GetInstructorCoursesAsync(instructorId);

            var recent = await GetRecentActivityAsync(instructorId, 6);

            return new InstructorDashboardDto
            {
                CoursesCount = coursesCount,
                TotalStudents = totalStudents,
                AverageRating = Math.Round(avgRating, 2),
                TotalRevenue = totalRevenue,
                Courses = courses,
                RecentActivities = recent
            };
        }

        public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(string instructorId, int count = 5)
        {
            // جمع أحدث الأنشطة من Orders, Enrollments, Reviews المتعلقة بكورسات المدرّس
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Course.UserId == instructorId)
                .Select(o => new RecentActivityDto
                {
                    ActivityType = "Order",
                    Description = $"Order #{o.Id} - {o.Amount} {o.Currency} - {o.Status}",
                    Date = o.CreatedAt
                })
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Course.UserId == instructorId)
                .Select(e => new RecentActivityDto
                {
                    ActivityType = "Enrollment",
                    Description = $"Student {e.StudentId} enrolled in course #{e.CourseId}",
                    Date = e.EnrollDate
                })
                .ToListAsync();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.Course.UserId == instructorId)
                .Select(r => new RecentActivityDto
                {
                    ActivityType = "Review",
                    Description = $"Rating {r.Rating} for course #{r.CourseId}: {(r.Comment ?? string.Empty).Substring(0, Math.Min(80, (r.Comment ?? string.Empty).Length))}",
                    Date = r.CreatedAt
                })
                .ToListAsync();

            var all = orders.Cast<RecentActivityDto>()
                .Concat(enrollments)
                .Concat(reviews)
                .OrderByDescending(a => a.Date)
                .Take(count)
                .ToList();

            return all;
        }
    }
}
