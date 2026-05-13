using ELProject.DataAccess.Repositories.Interfaces;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IOrderRepository Orders { get; private set; } = null!;
        public ITransactionRepository Transactions { get; private set; } = null!;
        public IEnrollmentRepository Enrollments { get; private set; } = null!;
        public ICourseRepository Courses { get; private set; } = null!;
        public IUserRepository Users { get; private set; } = null!;
        public ILessonRepository Lessons { get; private set; } = null!;
        public ISectionRepository Sections { get; private set; } = null!;
        public IQuizRepository Quizzes { get; private set; } = null!; 

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Orders = new OrderRepository(_context);
            Transactions = new TransactionRepository(_context);
            Enrollments = new EnrollmentRepository(_context);
            Courses = new CourseRepository(_context);
            Lessons = new LessonRepository(_context);
            Users = new UserRepository(_context);
            Sections = new SectionRepository(_context);
            Quizzes = new QuizRepository(_context);   // ← أضف
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}