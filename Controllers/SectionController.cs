using ELProject.DataAccess.Repositories.Repos;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly SectionRepository secRepo;

        public SectionController(SectionRepository _secRepo)
        {
            secRepo = _secRepo;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSection([FromForm] CreateSectionDto dto)
        {
            var sectionId = await secRepo.CreateSection(dto);

            if (sectionId == -1)
                return BadRequest("Error creating section");

            return Ok(new { sectionId });
        }


        [HttpGet("{sectionId}")]
        public async Task<IActionResult> GetSectionById(int sectionId)
        {
            var section = await secRepo.GetSectionById(sectionId);
            if (section == null)
                return NotFound("Section not found");
            return Ok(section);
        }


        [HttpGet("{courseId}/sections-with-lessons")]
        public async Task<IActionResult> GetSectionsByCourseId(int courseId)
        {
            var sections = await secRepo.GetSectionsWithLessonsByCourseId(courseId);
            return Ok(sections);
        }


        [HttpGet("{sectionId}/course-metadata")]
        public async Task<IActionResult> GetSectionwithCourseById(int sectionId)
        {
            var section = await secRepo.GetSectionwithCourseById(sectionId);
            if (section == null)
                return NotFound("Section not found");
            return Ok(section);
        }


        [Authorize(Roles = "Instructor")]
        [HttpPut("update/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, UpdateSectionDto dto)
        {
            var result = await secRepo.UpdateSection(sectionId, dto);

            if (!result)
                return NotFound("Section not found or update failed");

            return Ok("Section updated successfully");
        }


        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var result = await secRepo.DeleteSection(sectionId);

            if (!result)
                return NotFound("Section not found or delete failed");

            return Ok("Section deleted successfully");
        }
    }
}