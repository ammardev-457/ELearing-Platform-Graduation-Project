using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Repos
{
    // Implementations
    public class OrderRepository : Repository<Order, long>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }
    }
}