using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Quizzes;
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
                    Explanation = q.Explanation,
                    Points = q.Points,
                    Options = q.Options.Select(o => new Option
                    {
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };
            await context.Quizzes.AddAsync(quiz);
            context.Questions.AddRange(quiz.Questions);
            context.Options.AddRange(quiz.Questions.SelectMany(q => q.Options));
            return quiz;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId) => await context.Quizzes.FindAsync(quizId);

        public async Task<Quiz?> GetQuizWithDetailsByIdAsync(int quizId) => await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<bool> IsInstructorCreatedQuiz(string instructorId, int quizId)
        {
            var quiz = await context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return false;

            return quiz.Course.UserId == instructorId;
        }

        public async Task<int> UpdateQuizData(int quizId, UpdateQuizDto dto)
        {
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            // Update quiz properties
            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.QuizType = dto.QuizType;
            quiz.TotalMarks = dto.TotalMarks;
            quiz.TimeLimitInMinutes = dto.TimeLimitInMinutes;
            quiz.Questions = dto.Questions.Select(q => new Question
            {
                QuestionText = q.QuestionText,
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation,
                Points = q.Points,
                Options = q.Options.Select(o => new Option
                {
                    Text = o.Text
                }).ToList()
            }).ToList();

            // Handle questions and options updates here as needed
            context.Quizzes.Update(quiz);
            context.Questions.UpdateRange(quiz.Questions);
            context.Options.UpdateRange(quiz.Questions.SelectMany(q => q.Options));
            return quiz.Id;
        }

        public async Task<Quiz?> GetQuizWithQuestionsOnlyAsync(int quizId) => await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<bool> HasStudentSubmittedAsync(string studentId, int quizId) => await context.StudentQuizzes
            .AnyAsync(sq => sq.UserId == studentId && sq.QuizId == quizId);

        public async Task SaveStudentQuizAsync(StudentQuiz studentQuiz) => await context.StudentQuizzes.AddAsync(studentQuiz);

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

        public async Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId)
        {
            return await context.Quizzes
                .Where(q => q.CourseId == courseId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<string> UpdateQuizAsync(int quizId, string instructorId, QuizDto dto)
        {
            var existingQuiz = await context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (existingQuiz == null)
                return "Quiz not found";
            
            if (existingQuiz.Course.UserId != instructorId)
                return "Unauthorized";

            // Update quiz properties
            existingQuiz.Title = dto.Title;
            existingQuiz.Description = dto.Description;
            existingQuiz.QuizType = dto.QuizType;
            existingQuiz.TotalMarks = dto.TotalMarks;
            existingQuiz.TimeLimitInMinutes = dto.TimeLimitInMinutes;

            try
            {
                context.Quizzes.Update(existingQuiz);
                await context.SaveChangesAsync();
                return "Quiz updated successfully";
            }
            catch
            {
                return "An error occurred while updating the quiz";
            }
        }
        
        public async Task<string> DeleteQuizAsync(string instructorId, int quizId)
        {
            var quiz = await context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return "Quiz not found";
            
            if (quiz.Course.UserId != instructorId)
                return "Unauthorized";

            context.Quizzes.Remove(quiz);
            await context.SaveChangesAsync();
            return "Quiz deleted successfully";
        }
    }
}