using ELProject.Domain.Models;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction, long> { }
}