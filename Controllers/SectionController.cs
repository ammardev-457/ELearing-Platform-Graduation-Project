using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Courses;
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
        [HttpPost("create")]
        public async Task<IActionResult> CreateSection(CreateSectionDto dto)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (instructorId == null) return Unauthorized("User Not Authenticated");

            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
            if (course == null) return NotFound("Course not found");
            if (course.UserId != instructorId) return Forbid();

            var sectionId = await _unitOfWork.Sections.CreateSection(dto);
            await _unitOfWork.CompleteAsync();

            return Ok(new { sectionId });
        }

        [HttpGet("{sectionId}")]
        public async Task<IActionResult> GetSectionById(int sectionId)
        {
            var section = await _unitOfWork.Sections.GetSectionById(sectionId);
            if (section == null) return NotFound("Section not found");
            return Ok(section);
        }

        [HttpGet("{courseId}/sections-with-lessons")]
        public async Task<IActionResult> GetSectionsByCourseId(int courseId)
        {
            var sections = await _unitOfWork.Sections.GetSectionsWithLessonsByCourseId(courseId);
            return Ok(sections);
        }

        [HttpGet("{sectionId}/course-metadata")]
        public async Task<IActionResult> GetSectionwithCourseById(int sectionId)
        {
            var section = await _unitOfWork.Sections.GetSectionwithCourseById(sectionId);
            if (section == null) return NotFound("Section not found");
            return Ok(section);
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

            var result = await _unitOfWork.Sections.UpdateSection(sectionId, dto);
            if (!result) return NotFound("Section not found or update failed");

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

            var result = await _unitOfWork.Sections.DeleteSection(sectionId);
            if (!result) return NotFound("Section not found or delete failed");

            await _unitOfWork.CompleteAsync();
            return Ok("Section deleted successfully");
        }
    }
}