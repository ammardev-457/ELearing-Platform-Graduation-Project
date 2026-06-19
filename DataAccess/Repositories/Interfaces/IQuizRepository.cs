using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Quizzes;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface IQuizRepository : IRepository<Quiz, int>
    {
        public Task<Quiz> CreateQuizAsync(QuizDto dto);
        public Task<Quiz?> GetQuizByIdAsync(int quizId);
        public Task<Quiz?> GetQuizWithDetailsByIdAsync(int quizId);
        public Task<bool> IsInstructorCreatedQuiz(string instructorId, int quizId);
        public Task<int> UpdateQuizData(int quizId, UpdateQuizDto dto);
        public Task<Quiz?> GetQuizWithQuestionsOnlyAsync(int quizId);
        public Task<bool> HasStudentSubmittedAsync(string studentId, int quizId);
        public Task SaveStudentQuizAsync(StudentQuiz studentQuiz);
        public Task<StudentQuizResultDto?> GetStudentQuizResultAsync(string studentId, int quizId);
        public Task<List<AllStudentResultDto>> GetAllStudentResultsAsync(int quizId);
        public Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId);
        public Task<string> UpdateQuizAsync(int quizId, string instructorId, QuizDto dto);
        public  Task<string> DeleteQuizAsync(string instructorId, int quizId);

        public Task<bool> IsExistsAsync(int quizId);
    }
}