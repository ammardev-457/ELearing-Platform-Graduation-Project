using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Student;

namespace ELProject.DataAccess.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser, string>
    {
        Task<StudentProfileDto?> GetStudentProfileAsync(string studentId); 
        Task<StudentDashboardDto?> GetStudentDashboardAsync(string studentId);
        Task<IReadOnlyList<StudentCoursesDto>> GetMyCoursesAsync(string studentId);
    }
}