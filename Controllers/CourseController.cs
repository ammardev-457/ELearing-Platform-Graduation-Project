using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudStorageService cloudService;

        public CoursesController(IUnitOfWork unitOfWork, ICloudStorageService _cloudService)
        {
            _unitOfWork = unitOfWork;
            cloudService = _cloudService;
        }


        [HttpPost("{courseId}/upload-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(int courseId, IFormFile image) // [FromForm] is optional
        { // form-data من ال Automatic Binding بيعملها ASP.NET Core ال IFormFile

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return NotFound("Course Not Found");

            var url = await cloudService.UploadFileAsync(image, FileType.Image);
            course.Thumbnail = url;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Thumbnail = url });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CreateCourseDto courseDto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var course = new Course
            {
                Title = courseDto.Title,
                Price = courseDto.Price,
                UserId = InstructorId,
                CategoryId = courseDto.CategoryId,
                CreatedDate = DateTime.UtcNow,
                Level = courseDto.Level,
                ShortDescription = courseDto.ShortDescription,
                LongDescription = courseDto.LongDescription
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.CompleteAsync();

            //CreatedAtAction: Returns 201 Created and adds 'Location' header to the response
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }


        // To add multiple roles, use a comma-separated string
        [Authorize(Roles = "Student,Admin,Instructor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            return course == null ? NotFound("Course Not Found") : Ok(course);
        }

        // [Authorize(Roles = "Student,Admin,Instructor")]
        // [HttpGet]
        // public async Task<IActionResult> GetAllCourses()
        // {
        //     var courses = await _unitOfWork.Courses.GetAllAsync();
        //     return Ok(courses);
        // }


        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile(string fileUrl, FileType type)
        {
            var result = await cloudService.DownloadFileAsync(fileUrl, type);

            if (result == null)
                return NotFound();

            return File(result.Value.stream, result.Value.contentType, result.Value.fileName);
        }


    }
}