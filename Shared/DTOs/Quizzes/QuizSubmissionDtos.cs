namespace ELProject.Shared.DTOs.Quizzes
{
    public class QuizSubmitDto
    {
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; } = string.Empty;
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
    }

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

    public class StudentQuizResultDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxPossibleScore { get; set; }
        public double Percentage { get; set; }
        public DateTime SubmitDate { get; set; }
    }

    public class AllStudentResultDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxPossibleScore { get; set; }
        public double Percentage { get; set; }
        public DateTime SubmitDate { get; set; }
    }
}