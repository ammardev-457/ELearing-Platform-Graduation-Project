using ELProject.Domain.Models;
using ELProject.Shared.Results;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course, int> 
    {
        Task<CourseResult?> GetCourseWithDataAsync(int id);
    }
}