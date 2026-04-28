using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string QuestionText { get; set; } = null!;

        public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

        public List<string> Options { get; set; } = null!; // How choose answer??

        public string CorrectAnswer { get; set; } = null!;

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
    }
}