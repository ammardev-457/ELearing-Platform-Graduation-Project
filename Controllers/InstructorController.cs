using ELProject.DataAccess.Repositories;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor")] // Roles اختياري: لو بتستخدم
    public class InstructorController : ControllerBase
    {
        private readonly IInstructorRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(IInstructorRepository repo, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        // GET: api/instructor/dashboard  (dashboard for current logged instructor)
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = _userManager.GetUserId(User) ?? throw new UnauthorizedAccessException();
            var dto = await _repo.GetInstructorDashboardAsync(userId);
            return Ok(dto);
        }

        // GET: api/instructor/courses
        [HttpGet("courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = _userManager.GetUserId(User) ?? throw new UnauthorizedAccessException();
            var courses = await _repo.GetInstructorCoursesAsync(userId);
            return Ok(courses);
        }

        // GET: api/instructor/{id}/dashboard  (public view for instructor)
        [HttpGet("{instructorId}/dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInstructorDashboard(string instructorId)
        {
            var dto = await _repo.GetInstructorDashboardAsync(instructorId);
            return Ok(dto);
        }

        // GET: api/instructor/recent-activity
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(int count = 5)
        {
            var userId = _userManager.GetUserId(User) ?? throw new UnauthorizedAccessException();
            var activities = await _repo.GetRecentActivityAsync(userId, count);
            return Ok(activities);
        }

        // ---- Additional endpoints commonly used in real e-learning apps ----
        // - CRUD for courses (Create/Update/Delete)
        // - Get students of a course
        // - Get course revenue / analytics per course
        // Implement these in the controller calling repository methods as needed.
    }
}
