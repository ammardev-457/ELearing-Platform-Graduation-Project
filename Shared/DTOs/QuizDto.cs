using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs
{
    public class QuizDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public QuizType QuizType { get; set; } = QuizType.Mcq;
        public int TotalMarks { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public int CourseId { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];
    }

    public class QuestionDto
    {
        public string QuestionText { get; set; } = null!;
        public string? Explanation { get; set; } = null!;
        public List<OptionDto> Options { get; set; } = null!;
    }

    public class OptionDto
    {
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
