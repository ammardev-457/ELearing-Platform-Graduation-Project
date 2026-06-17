using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class InstructorRepository : Repository<Course, int>, IInstructorRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService fileStorage;

        public InstructorRepository(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorage) : base(context)
        {
            _context = context;
            _userManager = userManager;
            this.fileStorage = fileStorage;
        }

        public async Task<IList<ApplicationUser>> GetAllInstructorsAsync()
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            return instructors;
        }

        public async Task<InstructorStatisticsDto> GetInstructorStatisticsAsync(string instructorId)
        {
            var coursesQuery = _context.Courses.Where(c => c.UserId == instructorId);
            var coursesCount = await coursesQuery.CountAsync();

            var totalStudents = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.Course.UserId == instructorId)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var avgRating = await _context.Reviews
                .Include(r => r.Course)
                .Where(r => r.Course.UserId == instructorId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0;

            var totalRevenue = await _context.Orders
                .Include(o => o.Course)
                .Where(o => o.Course.UserId == instructorId && o.Status == PaymentStatus.Success.ToString())
                .Select(o => (decimal?)o.Amount)
                .SumAsync() ?? 0;

            return new InstructorStatisticsDto
            {
                CoursesCount = coursesCount,
                TotalStudents = totalStudents,
                AverageRating = Math.Round(avgRating, 2),
                TotalRevenue = totalRevenue
            };
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
                    Level = c.Level,
                    Thumbnail = c.Thumbnail,
                    CategoryId = c.CategoryId,
                    StudentsCount = c.Enrollments.Count(),
                    Revenue = c.Orders.Where(o => o.Status == PaymentStatus.Success.ToString()).Sum(o => (decimal?)o.Amount) ?? 0
                })
                .ToListAsync();

            return courses;
        }

        public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(string instructorId, int count = 4)
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

        public async Task<InstructorProfileDto> GetInstructorProfileAsync(string instructorId)
        {
            var instructor = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == instructorId)
                .Select(u => new InstructorProfileDto
                {
                    Name = u.Name,
                    Email = u.Email,
                    Title = u.Title,
                    PathOfImage = u.PathOfImage,
                    AboutMe = u.AboutMe,
                    JoinDate = u.JoinDate,
                    Bio = u.Bio,
                    CoursesCount = u.CreatedCourses.Count(),
                    AverageRating = u.CreatedCourses.SelectMany(c => c.Reviews).Any() ? u.CreatedCourses.SelectMany(c => c.Reviews).Average(r => r.Rating) : 0
                })
                .FirstOrDefaultAsync();
            return instructor ?? throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");
        }

        public async Task<bool> EditInstructorProfileAsync(string instructorId, EditInstructorProfileDto dto)
        {
            var instructor = await _context.Users.FindAsync(instructorId);
            
            if (instructor == null)
                return false;

            instructor.Name = dto.Name ?? instructor.Name;
            instructor.Email = dto.Email ?? instructor.Email;
            instructor.Title = dto.Title ?? "";
            instructor.Bio = dto.Bio ?? "";
            instructor.AboutMe = dto.AboutMe ?? "";
            instructor.Gender = dto.Gender ?? instructor.Gender;
            if(dto.Image != null)
            {
                if(instructor.PathOfImage != null)
                    await fileStorage.DeleteFileAsync(instructor.PathOfImage, FileType.Image);

                var imagePath = await fileStorage.UploadFileAsync(dto.Image, FileType.Image);
                instructor.PathOfImage = imagePath;
            }

            _context.Users.Update(instructor);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
