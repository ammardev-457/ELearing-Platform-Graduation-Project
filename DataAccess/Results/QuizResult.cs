namespace ELProject.DataAccess.Results
{
    public class QuizResult
    {
        public int QuizId { get; set; }

        public int TotalMarks { get; set; }

        public int Score { get; set; }

        public List<QuestionResultDto> Questions { get; set; } = [];
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public List<int> SelectedOptionIds { get; set; } = [];

        public List<int> CorrectOptionIds { get; set; } = [];
    }
}