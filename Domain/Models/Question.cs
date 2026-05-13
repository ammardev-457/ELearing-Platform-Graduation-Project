namespace ELProject.Domain.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;
        public int Points { get; set; } = 1;

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public List<Option> Options { get; set; } = [];
    }
}