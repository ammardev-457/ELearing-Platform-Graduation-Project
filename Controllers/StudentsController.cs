
using System.Reflection.Metadata.Ecma335;
using ELProject.DataAccess.Repositories;
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

        [HttpGet("{studentId}/Profile")]
        public async Task<IActionResult> GetProfile(string studentId)
        {
            var profile = await _unitOfWork.Users.GetStudentProfileAsync(studentId);
            return profile == null? 
                NotFound(new { message = $"No profile found for Student ID: {studentId}" }) : 
                Ok(profile);
        }

        [HttpGet("{studentId}/Dashboard")]
        public async Task<IActionResult> GetDashboard(string studentId)
        {
            var dashboard =  await _unitOfWork.Users.GetStudentDashboardAsync(studentId);
            return dashboard == null? NotFound(new {message = $"No dashboard found for Student ID: {studentId}"}) : Ok(dashboard);
        }

        [HttpGet("{studentId}/Courses")]
        public async Task<IActionResult> GetMyCourses(string studentId)
        {
            var myCourses = await _unitOfWork.Users.GetMyCoursesAsync(studentId);

            return myCourses.Count == 0? NotFound(new {message = $"No courses found for Student ID: {studentId}"}) : Ok(myCourses);
        }
    }
}