
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
    }
}