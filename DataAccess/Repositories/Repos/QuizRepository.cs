using ELProject.DataAccess.Results;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class QuizRepository
    {
        private readonly AppDbContext context;

        public QuizRepository(AppDbContext _context)
        {
            context = _context;
        }

        public async Task<Quiz> CreateQuizAsync(QuizDto dto)
        {
            // 1. Mapping DTO to Entity hierarchy
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
                    Explanation = q.Explanation,
                    Options = q.Options.Select(opt => new Option
                    {
                        Text = opt.Text,
                    }).ToList()
                }).ToList()
            };

            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            return quiz;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await context.Quizzes.FindAsync(quizId);
        }

        public async Task<Quiz?> GetQuizWithDetailsByIdAsync(int quizId)
        {
            return await context.Quizzes
                .AsNoTracking()
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

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
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
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
