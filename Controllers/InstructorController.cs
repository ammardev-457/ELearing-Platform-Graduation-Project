using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor")] 
    public class InstructorController : ControllerBase
    {
        private readonly InstructorRepository _repo;

        public InstructorController(InstructorRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetMyStatistics()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User not authenticated");

            var statistics = await _repo.GetInstructorStatisticsAsync(instructorId);
            return Ok(statistics);
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User not authenticated");

            var courses = await _repo.GetInstructorCoursesAsync(instructorId);
            return Ok(courses);
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(int count = 4)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User not authenticated");

            var activities = await _repo.GetRecentActivityAsync(instructorId, count);
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
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User not authenticated");

            var result = await _repo.EditInstructorProfileAsync(instructorId, dto);
            if (!result)
                return Unauthorized("Instructor does not have permission to edit this profile.");
            return CreatedAtAction(nameof(GetInstructorProfile), new { instructorId }, dto);
        }
    }
}

// ---- Additional endpoints commonly used in real e-learning apps ----
        // - CRUD for courses (Create/Update/Delete)
        // - Get students of a course
        // - Get course revenue / analytics per course
        // Implement these in the controller calling repository methods as needed.
