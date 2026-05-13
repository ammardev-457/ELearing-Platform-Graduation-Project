using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor")] 
    public class InstructorController : ControllerBase
    {
        private readonly InstructorRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(InstructorRepository repo, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetMyStatistics()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized("User not authenticated");

            var statistics = await _repo.GetInstructorStatisticsAsync(userId);
            return Ok(statistics);
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized("User not authenticated");

            var courses = await _repo.GetInstructorCoursesAsync(userId);
            return Ok(courses);
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(int count = 4)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized("User not authenticated");

            var activities = await _repo.GetRecentActivityAsync(userId, count);
            return Ok(activities);
        }

        [HttpGet("{instructorId}/profile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInstructorProfile(string instructorId)
        {
            var dto = await _repo.GetInstructorProfileAsync(instructorId);
            return Ok(dto);
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromForm] EditInstructorProfileDto dto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized("User not authenticated");

            var result = await _repo.EditInstructorProfileAsync(userId, dto);
            if (!result)
                return Unauthorized("Instructor does not have permission to edit this profile.");
            return CreatedAtAction(nameof(GetInstructorProfile), new { instructorId = userId }, dto);
        }
    }
}

// ---- Additional endpoints commonly used in real e-learning apps ----
        // - CRUD for courses (Create/Update/Delete)
        // - Get students of a course
        // - Get course revenue / analytics per course
        // Implement these in the controller calling repository methods as needed.
