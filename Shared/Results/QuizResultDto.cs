using ELProject.Shared.DTOs.Quizzes;

namespace ELProject.Shared.Results
{
    public class QuizResultDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxPossibleScore { get; set; }
        public double Percentage { get; set; }
        public DateTime SubmitDate { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new();
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
