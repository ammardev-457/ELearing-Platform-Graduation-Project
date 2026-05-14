using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Quizzes
{
    public class UpdateQuizDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public QuizType QuizType { get; set; }
        public int TotalMarks { get; set; }
        public int TimeLimitInMinutes { get; set; }
        public int CourseId { get; set; }
        public List<UpdateQuestionDto> Questions { get; set; } = [];
    }

    public class UpdateQuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public List<UpdateOptionDto> Options { get; set; } = [];
        public int Points { get; set; } = 1;
    }

    public class UpdateOptionDto
    {
        public string Text { get; set; } = string.Empty;
    }

}
