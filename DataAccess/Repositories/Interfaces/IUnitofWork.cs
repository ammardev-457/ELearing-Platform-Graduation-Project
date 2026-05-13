namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        ITransactionRepository Transactions { get; }
        IEnrollmentRepository Enrollments { get; }
        ICourseRepository Courses { get; }
        ILessonRepository Lessons { get; }
        IUserRepository Users { get; }
        ISectionRepository Sections { get; }
        IQuizRepository Quizzes { get; }
        Task<int> CompleteAsync();
    }
}