using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {

        private readonly QuizRepository quizRepo;

        public QuizController(QuizRepository _quizRepo)
        {
            quizRepo = _quizRepo;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz(QuizDto dto)
        {
            if (dto == null) return BadRequest("Quiz data is required.");

            try
            {
                var createdQuiz = await quizRepo.CreateQuizAsync(dto);

                return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
            }
            catch (Exception ex)
            {
                // Log the error (with alert: something went boom)
                return StatusCode(500, "An error occurred while creating the quiz.");
            }

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await quizRepo.GetQuizByIdAsync(id);

            if (quiz == null)
                return NotFound();

            return quiz;
        }


        [Authorize]
        [HttpGet("{id}/quiz-data")]
        public async Task<IActionResult> GetQuizData(int id)
        {
            var quiz = await quizRepo.GetQuizWithDetailsByIdAsync(id);

            if (quiz == null)
                return NotFound();

            return Ok(new
            {
                id = quiz.Id,
                title = quiz.Title,
                description = quiz.Description,
                quizType = quiz.QuizType,
                totalMarks = quiz.TotalMarks,
                timeLimitInMinutes = quiz.TimeLimitInMinutes,
                courseId = quiz.CourseId,
                questions = quiz.Questions.Select(q => new Question
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Explanation = q.Explanation,
                    Options = q.Options.Select(opt => new Option
                    {
                        Id = opt.Id,
                        Text = opt.Text
                    }).ToList()
                }).ToList()
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetQuizzesByCourseId(int courseId)
        {
            var quizzes = await quizRepo.GetQuizzesByCourseIdAsync(courseId);
            
            if (quizzes == null || !quizzes.Any())
                return NotFound();
            
            return Ok(quizzes);
        }


        [Authorize(Roles = "Instructor")]
        [HttpPut("{quizId}/update")]
        public async Task<IActionResult> UpdateQuiz(int quizId, QuizDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(InstructorId == null) return Unauthorized("User Not Authenticated");

            if (dto == null) return BadRequest("Quiz data is required.");

            var updateResult = await quizRepo.UpdateQuizAsync(quizId, InstructorId, dto);

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

            await quizRepo.DeleteQuizAsync(InstructorId, quizId);

            return Ok("Quiz Deleted Successfully");
        }
    }
}