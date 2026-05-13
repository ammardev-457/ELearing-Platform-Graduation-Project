using ELProject.DataAccess.Repositories.Repos;
using ELProject.Shared.DTOs;
using ELProject.Shared.DTOs.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> CreateSection(CreateSectionDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var sectionId = await secRepo.CreateSection(dto);

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

    }
}