using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.Shared.Quiz.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class QuizRepository : Repository<Quiz, int>, IQuizRepository
    {
        private readonly AppDbContext context;
        public QuizRepository(AppDbContext _context) : base(_context) => context = _context;

        public async Task<Quiz> CreateQuizAsync(QuizDto dto)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                QuizType = dto.QuizType,
                TotalMarks = dto.TotalMarks,
                TimeLimitInMinutes = dto.TimeLimitInMinutes,
                CourseId = dto.CourseId,
                Questions = dto.Questions.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points,
                    Options = q.Options.Select(opt => new Option { Text = opt }).ToList()
                }).ToList()
            };
            context.Quizzes.Add(quiz);
            return quiz;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId) => await context.Quizzes.FindAsync(quizId);
        public async Task<Quiz?> GetQuizWithDetailsByIdAsync(int quizId) => await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<Quiz?> GetQuizWithQuestionsOnlyAsync(int quizId) => await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<bool> HasStudentSubmittedAsync(string studentId, int quizId) => await context.StudentQuizzes
            .AnyAsync(sq => sq.UserId == studentId && sq.QuizId == quizId);

        public async Task SaveStudentQuizAsync(StudentQuiz studentQuiz) => context.StudentQuizzes.Add(studentQuiz);

        public async Task<StudentQuizResultDto?> GetStudentQuizResultAsync(string studentId, int quizId) => await context.StudentQuizzes
            .Where(sq => sq.UserId == studentId && sq.QuizId == quizId)
            .Include(sq => sq.Quiz)
            .Select(sq => new StudentQuizResultDto
            {
                QuizId = sq.QuizId,
                QuizTitle = sq.Quiz.Title,
                Score = sq.Score,
                MaxPossibleScore = sq.Quiz.Questions.Sum(q => q.Points),
                Percentage = (double)sq.Score / sq.Quiz.Questions.Sum(q => q.Points) * 100,
                SubmitDate = sq.SubmitDate
            })
            .FirstOrDefaultAsync();

        public async Task<List<AllStudentResultDto>> GetAllStudentResultsAsync(int quizId) => await context.StudentQuizzes
            .Where(sq => sq.QuizId == quizId)
            .Include(sq => sq.Quiz)
            .Include(sq => sq.User)
            .Select(sq => new AllStudentResultDto
            {
                StudentId = sq.UserId,
                StudentName = sq.User.Name,
                Score = sq.Score,
                MaxPossibleScore = sq.Quiz.Questions.Sum(q => q.Points),
                Percentage = (double)sq.Score / sq.Quiz.Questions.Sum(q => q.Points) * 100,
                SubmitDate = sq.SubmitDate
            })
            .ToListAsync();

    }
}