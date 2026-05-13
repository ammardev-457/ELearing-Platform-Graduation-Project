using ELProject.Domain.Models;
using ELProject.Shared.DTOs;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment, int>
    {
        Task<Enrollment?> ExistsAsync(string studentId, int courseId);

        Task<bool> IsFoundAsync(string studentId, int courseId);
    }
}