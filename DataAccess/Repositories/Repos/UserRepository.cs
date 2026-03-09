using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Student;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class UserRepository : Repository<ApplicationUser, string>, IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<StudentCoursesDto>> GetMyCoursesAsync(string studentId)
        {
            var myCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => new StudentCoursesDto
                {
                    Title = e.Course.Title,
                    Thumbnail = string.Empty,
                    Category = e.Course.Category.Name,

                    Rate = e.Course.Reviews.Any()
                        ? e.Course.Reviews.Average(r => r.Rating)
                        : 0,

                    LessonsCount = e.Course.Sections
                        .SelectMany(s => s.Lessons)
                        .Count(),

                    Hours = 0,
                    Progress = e.Progress
                })
                .ToListAsync();

            return myCourses;
        }

        public async Task<StudentDashboardDto?> GetStudentDashboardAsync(string studentId)
        {

            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .Select(e => new StudentDashboardCourse
                {
                    CourseName = e.Course.Title,
                    PictureUrl = null,
                    InstructorName = e.Course.User.UserName ?? "UnKnow",
                    Progress = (int)e.Progress
                })
                .ToListAsync();

            var dashboard = new StudentDashboardDto
            {
                EnrollmentCourses = enrollments.Count,
                Completed = enrollments.Count(e => e.Progress == 100),
                InProgressCount = enrollments.Count(e => e.Progress > 0 && e.Progress < 100),
                LearningHours = 0,
                Courses = enrollments
            };

            return dashboard;
        }

        public async Task<StudentProfileDto?> GetStudentProfileAsync(string studentId)
        {
            var userProfile = await _context.ApplicationUsers
                .AsNoTracking()
                .Where(u => u.Id == studentId)
                .Select(u => new StudentProfileDto
                {
                    Email = u.Email!,
                    Username = u.UserName!,
                    Bio = u.Bio,
                    CoursesCount = u.Enrollments.Count
                })
                .FirstOrDefaultAsync();

            return userProfile;
        }
    }


}