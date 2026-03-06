using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class EnrollmentRepository : Repository<Enrollment, int>, IEnrollmentRepository
    {
        private readonly AppDbContext _context;

        public EnrollmentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Enrollment?> ExistsAsync(string studentId, int courseId)
        {
            Enrollment? enrollment =  await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            
            return enrollment;
        }
    }
}