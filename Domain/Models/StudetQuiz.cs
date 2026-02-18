namespace ELProject.Domain.Models
{
    public class StudentQuiz
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public int Score { get; set; }
        public DateTime SubmitDate { get; set; }
    }
}