using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        public string Title { get; set; } = null!;

        public QuizType QuizType { get; set; } = QuizType.Mcq;

        public int TotalMarks { get; set; }

        public int TimeLimitInMinutes { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<Question> Questions { get; set; } = [];
        public ICollection<StudentQuiz> StudentQuizzes { get; set; } = [];
    }
}