using ELProject.DataAccess.Results;
using ELProject.Domain.Models;
using ELProject.Shared.Quiz.DTOs;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface IQuizRepository : IRepository<Quiz, int>
    {
        public Task<Quiz> CreateQuizAsync(QuizDto dto);
        public Task<Quiz?> GetQuizByIdAsync(int quizId);
        public Task<Quiz?> GetQuizWithDetailsByIdAsync(int quizId);
        public Task<Quiz?> GetQuizWithQuestionsOnlyAsync(int quizId);
        public Task<bool> HasStudentSubmittedAsync(string studentId, int quizId);
        public Task SaveStudentQuizAsync(StudentQuiz studentQuiz);
        public Task<StudentQuizResultDto?> GetStudentQuizResultAsync(string studentId, int quizId);
        public Task<List<AllStudentResultDto>> GetAllStudentResultsAsync(int quizId);
    }
}