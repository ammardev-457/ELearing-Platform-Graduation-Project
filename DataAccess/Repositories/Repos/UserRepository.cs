using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class UserRepository : Repository<ApplicationUser, string>, IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}