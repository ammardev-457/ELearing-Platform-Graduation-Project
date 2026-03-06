using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class TransactionRepository : Repository<Transaction, long>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context) { }
    }
}