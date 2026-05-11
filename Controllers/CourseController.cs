using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Courses;
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
        private readonly IFileStorageService fileService;

        public CoursesController(IUnitOfWork unitOfWork, IFileStorageService _fileService)
        {
            _unitOfWork = unitOfWork;
            fileService = _fileService;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDto courseDto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            string? thumbnailUrl = null;

            if (courseDto.Thumbnail != null)
                thumbnailUrl = await fileService.UploadFileAsync(courseDto.Thumbnail, FileType.Image);

            var course = new Course
            {
                Thumbnail = thumbnailUrl,
                Title = courseDto.Title,
                Price = courseDto.Price,
                UserId = InstructorId,
                CategoryId = courseDto.CategoryId,
                CreatedDate = DateTime.UtcNow,
                Level = courseDto.Level,
                ShortDescription = courseDto.ShortDescription,
                LongDescription = courseDto.LongDescription
            };

            try
            {
                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.CompleteAsync();

                return Ok(new { courseId = course.Id });
            }
            catch
            {
                if (thumbnailUrl != null)
                    await fileService.DeleteFileAsync(thumbnailUrl, FileType.Image);
                
                return StatusCode(500, "An error occurred while creating the course. Please try again.");   
            }
        }


        /// <remarks>
        /// Get all published and active courses on the platform with pagination. Example: GET /api/courses?PagedNumber=1&PagedSize=10
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCourses([FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAsync(null, paginationParams.PagedNumber, paginationParams.PagedSize);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor")]
        [HttpGet("{courseId}/course-metadata")]
        public async Task<IActionResult> GetCourseMetaData(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != course.UserId) return Unauthorized("User Not Authenticated");

            return course == null ? NotFound("Course Not Found") : Ok( new 
            { 
                title = course.Title,
                shortDescription = course.ShortDescription,
                longDescription = course.LongDescription,
                thumbnail = course.Thumbnail,
                createdDate = course.CreatedDate,
                level = course.Level,
                price = course.Price,
                rate = course.Rate,
                categoryId = course.CategoryId
            });
        }


        [AllowAnonymous]
        [HttpGet("{courseId}/course-data")]
        public async Task<IActionResult> GetPaidCourseWithData(int courseId)
        {
            var paidCourse = await _unitOfWork.Courses.GetPaidCourseWithDataAsync(courseId);

            if (paidCourse == null)
                return NotFound("Course Not Found");

            paidCourse.InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            paidCourse.InstructorName = User.FindFirstValue(ClaimTypes.Name);

            return Ok(paidCourse);
        }


        [Authorize(Roles = "Student")]
        [HttpGet("{courseId}/enrolled-course")]
        public async Task<IActionResult> GetEnrolledCourseWithData(int courseId)
        {
            var course = await _unitOfWork.Courses.GetEnrolledCourseWithDataAsync(courseId);

            if (course == null)
                return NotFound("Course Not Found");

            return Ok(course);
        }


        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetCoursesByCategory(int categoryId, [FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAsync(
                c => c.CategoryId == categoryId,
                paginationParams.PagedNumber,
                paginationParams.PagedSize);
            return Ok(result);
        }


        [HttpGet("by-instructor/{instructorId}")]
        public async Task<IActionResult> GetCoursesByInstructor(string instructorId, [FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAsync(
                c => c.UserId == instructorId,
                paginationParams.PagedNumber,
                paginationParams.PagedSize);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor")]
        [HttpGet("download-file")]
        public async Task<IActionResult> DownloadFile(string fileUrl, FileType type)
        {
            var result = await fileService.DownloadFileAsync(fileUrl, type);

            if (result == null)
                return NotFound();

            return File(result.Value.stream, result.Value.contentType, result.Value.fileName);
        }


        [Authorize(Roles = "Instructor")]
        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromForm] UpdateCourseDto courseDto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return NotFound("Course Not Found");

            // Only the instructor who created the course can update it
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.UserId != userId)
                return Unauthorized("You do not have permission to update this course.");

            course.Title = courseDto.Title;
            course.Price = courseDto.Price;
            course.CategoryId = courseDto.CategoryId;
            course.Level = courseDto.Level;
            course.ShortDescription = courseDto.ShortDescription;
            course.LongDescription = courseDto.LongDescription;
            if (courseDto.Thumbnail != null)
            {
                if (course.Thumbnail != null)
                    await fileService.DeleteFileAsync(course.Thumbnail, FileType.Image);

                course.Thumbnail = await fileService.UploadFileAsync(courseDto.Thumbnail, FileType.Image);
            }

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetCourseMetaData), new { id = course.Id }, course);
        }


        [Authorize(Roles = "Instructor")]
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return NotFound("Course Not Found");

            // Only the instructor who created the course can delete it
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.UserId != userId)
                return Unauthorized("You do not have permission to delete this course.");

            if (course.Thumbnail != null)
                await fileService.DeleteFileAsync(course.Thumbnail, FileType.Image);

            _unitOfWork.Courses.Remove(course);
            await _unitOfWork.CompleteAsync();
            return Ok("Course Deleted Successfully");
        }

    }
}