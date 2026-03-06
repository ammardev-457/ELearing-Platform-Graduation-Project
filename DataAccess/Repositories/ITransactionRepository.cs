using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction, long> { }
}