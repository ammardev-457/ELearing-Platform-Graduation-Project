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
                    CorrectAnswer = q.CorrectAnswer,
                    // Map the strings from DTO to Option entities
                    Options = q.Options.Select(opt => new Option
                    {
                        Text = opt
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

    }
}
