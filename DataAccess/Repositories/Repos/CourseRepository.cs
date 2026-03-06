using System.Linq.Expressions;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class CourseRepository : Repository<Course, int> , ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}