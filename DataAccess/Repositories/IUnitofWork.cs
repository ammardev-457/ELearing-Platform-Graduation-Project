namespace ELProject.DataAccess.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        ITransactionRepository Transactions { get; }
        IEnrollmentRepository Enrollments { get; }
        ICourseRepository Courses { get; }
        IUserRepository Users{get;}
        Task<int> CompleteAsync();
    }
}