
using System.Reflection.Metadata.Ecma335;
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

            var profile = await _unitOfWork.Users.GetStudentProfileAsync(studentId);
            return profile == null? 
                NotFound(new { message = $"No profile found for Student ID: {studentId}" }) : 
                Ok(profile);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dashboard =  await _unitOfWork.Users.GetStudentDashboardAsync(studentId);
            return dashboard == null? NotFound(new {message = $"No dashboard found for Student ID: {studentId}"}) : Ok(dashboard);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myCourses = await _unitOfWork.Users.GetMyCoursesAsync(studentId);

            return myCourses.Count == 0? NotFound(new {message = $"No courses found for Student ID: {studentId}"}) : Ok(myCourses);
        }
    }
}