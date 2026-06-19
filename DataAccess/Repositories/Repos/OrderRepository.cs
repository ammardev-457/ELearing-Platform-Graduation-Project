using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    // Implementations
    public class OrderRepository : Repository<Order, long>, IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order?> FindOrderByPaymentReferenceAsync(string paymentReference)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.PaymentRefernce == paymentReference);
        }
    }
}