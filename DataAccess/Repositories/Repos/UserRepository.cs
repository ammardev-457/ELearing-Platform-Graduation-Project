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