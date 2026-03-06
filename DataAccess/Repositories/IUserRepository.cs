using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser, string> 
    {}
}