using ELProject.DataAccess.Results;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course, int> 
    {
        Task<PaidCourseResult> GetPaidCourseWithDataAsync(int id);
        Task<Course?> GetEnrolledCourseWithDataAsync(int id);
    }
}