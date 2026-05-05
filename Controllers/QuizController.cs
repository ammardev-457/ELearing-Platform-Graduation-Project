using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles ="Instructor")]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizDto dto)
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
            var quiz = await quizRepo.GetQuizWithDetailsByIdAsync(id);

            if (quiz == null)
                return NotFound();

            return quiz;
        }


        [Authorize]
        [HttpGet("{id}/quiz-data")]
        public async Task<ActionResult<Quiz>> GetQuizData(int id)
        {
            var quiz = await quizRepo.GetQuizWithDetailsByIdAsync(id);

            if (quiz == null)
                return NotFound();

            return quiz;
        }
    }
}