using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Quizzes
{
    public class QuizDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public QuizType QuizType { get; set; }
        public int TotalMarks { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public int CourseId { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }

    public class QuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public List<OptionDto> Options { get; set; } = [];
        public int Points { get; set; } = 1;
    }

    public class OptionDto
    {
        public string Text { get; set; } = string.Empty;
    }
}