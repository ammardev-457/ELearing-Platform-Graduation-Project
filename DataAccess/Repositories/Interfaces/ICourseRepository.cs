using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Course, int> 
    {
        Task<Course?> GetCourseWithDataAsync(int id);
    }
}