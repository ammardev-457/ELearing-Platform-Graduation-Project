using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Quizzes;
using ELProject.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuizController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz(QuizDto dto)
        {
            if (dto == null) return BadRequest("Quiz data is required.");

            try
            {
                var createdQuiz = await _unitOfWork.Quizzes.CreateQuizAsync(dto);
                await _unitOfWork.CompleteAsync();
                return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
            }
            catch
            {
                return StatusCode(500, "An error occurred while creating the quiz.");
            }
        }

        [Authorize]
        [HttpGet("{id}/metadata")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _unitOfWork.Quizzes.GetQuizWithDetailsByIdAsync(id);
            if (quiz == null) return NotFound();
            return quiz;
        }

        [Authorize]
        [HttpGet("{id}/quiz-data")]
        public async Task<IActionResult> GetQuizData(int id)
        {
            var quiz = await _unitOfWork.Quizzes.GetQuizWithDetailsByIdAsync(id);
            if (quiz == null) return NotFound();
            return Ok(quiz);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("course/{courseId}/quiz/{quizId}/update-quiz-data")]
        public async Task<IActionResult> UpdateQuizData(int courseId, int quizId, [FromBody] UpdateQuizDto dto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User not authenticated");

            var IsInstructorOwnedQuiz = await _unitOfWork.Quizzes.IsInstructorCreatedQuiz(instructorId, quizId);
            if (!IsInstructorOwnedQuiz) return Forbid("You are not authorized to update this quiz");

            try
            {
                await _unitOfWork.Quizzes.UpdateQuizData(quizId, dto);
                await _unitOfWork.CompleteAsync();
                return CreatedAtAction(nameof(GetQuizData), new { id = quizId }, null);
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the quiz data.");
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitQuiz(int id, [FromBody] QuizSubmitDto submitDto)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("User not authenticated");

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsOnlyAsync(id);
            if (quiz == null)
                return NotFound("Quiz not found");

            var alreadySubmitted = await _unitOfWork.Quizzes.HasStudentSubmittedAsync(studentId, id);
            if (alreadySubmitted)
                return BadRequest("You have already submitted this quiz");

            int totalScore = 0;
            int maxPossibleScore = 0;
            var questionResults = new List<QuestionResultDto>();

            foreach (var question in quiz.Questions)
            {
                maxPossibleScore += question.Points;
                var userAnswer = submitDto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                if (userAnswer != null && question.CorrectAnswer == userAnswer.SelectedAnswer)
                {
                    totalScore += question.Points;
                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        IsCorrect = true,
                        PointsEarned = question.Points,
                        CorrectAnswer = question.CorrectAnswer
                    });
                }
                else
                {
                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        IsCorrect = false,
                        PointsEarned = 0,
                        CorrectAnswer = question.CorrectAnswer
                    });
                }
            }

            var studentQuiz = new StudentQuiz
            {
                UserId = studentId,
                QuizId = id,
                Score = totalScore,
                SubmitDate = DateTime.UtcNow
            };

            await _unitOfWork.Quizzes.SaveStudentQuizAsync(studentQuiz);
            await _unitOfWork.CompleteAsync();

            return Ok(new QuizResultDto
            {
                QuizId = id,
                QuizTitle = quiz.Title,
                Score = totalScore,
                MaxPossibleScore = maxPossibleScore,
                Percentage = (double)totalScore / maxPossibleScore * 100,
                SubmitDate = studentQuiz.SubmitDate,
                QuestionResults = questionResults
            });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("{id}/my-result")]
        public async Task<IActionResult> GetMyQuizResult(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("User not authenticated");

            var result = await _unitOfWork.Quizzes.GetStudentQuizResultAsync(studentId, id);
            if (result == null)
                return NotFound("You haven't submitted this quiz yet");

            return Ok(result);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("{id}/all-results")]
        public async Task<IActionResult> GetAllQuizResults(int id)
        {
            var quiz = await _unitOfWork.Quizzes.GetQuizByIdAsync(id);
            if (quiz == null)
                return NotFound("Quiz not found");

            var results = await _unitOfWork.Quizzes.GetAllStudentResultsAsync(id);
            return Ok(results);
        }


        [HttpGet("course/{courseId}/quizzes")]
        public async Task<IActionResult> GetQuizzesByCourseId(int courseId)
        {
            var quizzes = await _unitOfWork.Quizzes.GetQuizzesByCourseIdAsync(courseId);
            return Ok(quizzes);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("{quizId}/update-quiz-metadata")]
        public async Task<IActionResult> UpdateQuiz(int quizId, QuizDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            if (dto == null) return BadRequest("Quiz data is required.");

            var updateResult = await _unitOfWork.Quizzes.UpdateQuizAsync(quizId, InstructorId, dto);

            if (updateResult == "Quiz not found")
                return NotFound();

            if (updateResult == "Unauthorized")
                return Unauthorized();

            if (updateResult == "An error occurred while updating the quiz")
                return StatusCode(500, updateResult);

            return CreatedAtAction(nameof(GetQuiz), new { id = quizId }, dto);
        }


        [Authorize(Roles = "Instructor")]
        [HttpDelete("{quizId}/delete")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var deleteResult = await _unitOfWork.Quizzes.DeleteQuizAsync(InstructorId, quizId);

            if (deleteResult == "Quiz not found")
                return NotFound();

            if (deleteResult == "Unauthorized")
                return Unauthorized();

            if (deleteResult == "An error occurred while deleting the quiz")
                return StatusCode(500, deleteResult);

            return Ok("Quiz Deleted Successfully");
        }
    }
}