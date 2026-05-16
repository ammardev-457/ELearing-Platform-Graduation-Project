using System.Security.Claims;
using ELProject.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [ApiController]
    [Authorize(Roles = "Student")]
    [Route("api/[controller]")]
    public class StudentsController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var profile = await _unitOfWork.Users.GetStudentProfileAsync(studentId);
            return profile == null
                ? NotFound(new { message = "Student profile not found." })
                : Ok(profile);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var dashboard = await _unitOfWork.Users.GetStudentDashboardAsync(studentId);
            return dashboard == null
                ? NotFound(new { message = "Student dashboard not found." })
                : Ok(dashboard);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var myCourses = await _unitOfWork.Users.GetMyCoursesAsync(studentId);

            // Return empty list instead of 404 — student simply has no courses yet
            return Ok(myCourses);
        }
    }
}