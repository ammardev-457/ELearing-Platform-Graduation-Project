using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Shared.DTOs.Courses;
using ELProject.Shared.DTOs.Sections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SectionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("course/{courseId}/create")]
        public async Task<IActionResult> CreateSection(int courseId, [FromBody] CreateSectionDto dto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User Not Authenticated");

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return NotFound("Course not found");

            if (course.UserId != instructorId) return Forbid();

            var sectionId = await _unitOfWork.Sections.CreateSection(courseId, dto);

            return Ok(new { sectionId });
        }

        [HttpGet("{sectionId}")]
        public async Task<IActionResult> GetSectionById(int sectionId)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
            if (section == null) return NotFound("Section not found");
            return Ok(new
            {
                id = section.Id,
                title = section.Title,
                order = section.Order,
                courseId = section.CourseId
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, UpdateSectionDto dto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User Not Authenticated");

            var section = await _unitOfWork.Sections.GetSectionwithCourseById(sectionId);
            if (section == null) return NotFound("Section not found");
            if (section.Course.UserId != instructorId) return Forbid();

            var result = await _unitOfWork.Sections.UpdateSection(section, dto);
            if (!result) return NotFound("Section update failed");

            _unitOfWork.Sections.Update(section);
            await _unitOfWork.CompleteAsync();
            return Ok("Section updated successfully");
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User Not Authenticated");

            var section = await _unitOfWork.Sections.GetSectionwithCourseById(sectionId);
            if (section == null) return NotFound("Section not found");
            if (section.Course.UserId != instructorId) return Forbid();

            try
            {
                _unitOfWork.Sections.Remove(section);
                await _unitOfWork.CompleteAsync();
                return Ok("Section deleted successfully");
            }
            catch
            {
                return NotFound("Section deleted failed");
            }
        }
    }
}