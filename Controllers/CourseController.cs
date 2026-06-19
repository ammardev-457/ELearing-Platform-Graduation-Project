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
using System.Threading.Tasks;

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
        [HttpGet]
        public async Task<IActionResult> GetAllCourses([FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAllCoursesAsync(paginationParams.PagedNumber, paginationParams.PagedSize);

            result.Items.ForEach(async c =>
            {
                if (!string.IsNullOrEmpty(c.Thumbnail))
                    c.Thumbnail = await fileService.GenerateSasUrlAsync(c.Thumbnail, FileType.Image, 1440, true);
            });

            return Ok(result.Items.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                shortDescription = c.ShortDescription,
                longDescription = c.LongDescription,
                thumbnail = c.Thumbnail,
                createdDate = c.CreatedDate,
                level = c.Level,
                price = c.Price,
                rate = c.Rate,
                userId = c.UserId,
                categoryId = c.CategoryId,
                categoryName = c.Category.Name
            }));
        }


        [Authorize(Roles = "Instructor")]
        [HttpGet("{courseId}/course-metadata")]
        public async Task<IActionResult> GetCourseMetaData(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);

            if (course == null)
                return NotFound("Course Not Found");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != course.UserId) return Unauthorized("User Not Authenticated");

            var thumbnail = await fileService.GenerateSasUrlAsync(course.Thumbnail!, FileType.Image, 1440);

            return Ok(new
            {
                thumbnail,
                title = course.Title,
                shortDescription = course.ShortDescription,
                longDescription = course.LongDescription,
                createdDate = course.CreatedDate,
                level = course.Level,
                price = course.Price,
                rate = course.Rate,
                categoryId = course.CategoryId
            });
        }


        [HttpGet("{courseId}/course-data")]
        public async Task<IActionResult> GetCourseData(int courseId)
        {
            var course = await _unitOfWork.Courses.GetCourseWithDataAsync(courseId);
            
            if (course == null)
                return NotFound("Course Not Found");

            var thumbnail = await fileService.GenerateSasUrlAsync(course.Thumbnail!, FileType.Image, 1440);
            course.Thumbnail = thumbnail;

            return Ok(course);
        }


        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetCoursesByCategory(int categoryId, [FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAsync(
                c => c.CategoryId == categoryId,
                paginationParams.PagedNumber,
                paginationParams.PagedSize);

            result.Items.ForEach(async c =>
            {
                if (!string.IsNullOrEmpty(c.Thumbnail))
                    c.Thumbnail = await fileService.GenerateSasUrlAsync(c.Thumbnail, FileType.Image, 1440, true);
            });

            return Ok(result.Items.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                shortDescription = c.ShortDescription,
                longDescription = c.LongDescription,
                thumbnail = c.Thumbnail,
                createdDate = c.CreatedDate,
                level = c.Level,
                price = c.Price,
                rate = c.Rate,
                userId = c.UserId,
                categoryId = c.CategoryId
            }));
        }


        [HttpGet("by-instructor/{instructorId}")]
        public async Task<IActionResult> GetCoursesByInstructor(string instructorId, [FromQuery] PaginationParameters paginationParams)
        {
            var result = await _unitOfWork.Courses.GetAsync(
                c => c.UserId == instructorId,
                paginationParams.PagedNumber,
                paginationParams.PagedSize);

            result.Items.ForEach(async c =>
            {
                if (!string.IsNullOrEmpty(c.Thumbnail))
                    c.Thumbnail = await fileService.GenerateSasUrlAsync(c.Thumbnail, FileType.Image, 1440, true);
            });

            return Ok(result.Items.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                shortDescription = c.ShortDescription,
                longDescription = c.LongDescription,
                thumbnail = c.Thumbnail,
                createdDate = c.CreatedDate,
                level = c.Level,
                price = c.Price,
                rate = c.Rate,
                userId = c.UserId,
                categoryId = c.CategoryId
            }));
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

            course.Title = courseDto.Title ?? course.Title;
            course.ShortDescription = courseDto.ShortDescription ?? course.ShortDescription;
            course.LongDescription = courseDto.LongDescription ?? course.LongDescription;
            course.Level = courseDto.Level ?? course.Level;
            course.CategoryId = courseDto.CategoryId == 0 ? course.CategoryId : courseDto.CategoryId;
            course.Price = courseDto.Price == 0 ? course.Price : courseDto.Price;

            if (courseDto.Thumbnail != null)
            {
                if (course.Thumbnail != null)
                    await fileService.DeleteFileAsync(course.Thumbnail, FileType.Image);

                course.Thumbnail = await fileService.UploadFileAsync(courseDto.Thumbnail, FileType.Image);
            }

            try
            {
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.CompleteAsync();

                return CreatedAtAction(nameof(GetCourseMetaData), new { courseId = course.Id }, course);
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the course. Please try again.");
            }
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

            _unitOfWork.Courses.Remove(course);
            await _unitOfWork.CompleteAsync();

            if (course.Thumbnail != null)
                await fileService.DeleteFileAsync(course.Thumbnail, FileType.Image);

            return Ok("Course Deleted Successfully");
        }

    }
}