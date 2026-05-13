using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs.Courses;
using ELProject.Shared.DTOs.Lessons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IFileStorageService fileService;

        public LessonController(IUnitOfWork unitOfWork, IFileStorageService _fileService)
        {
            this.unitOfWork = unitOfWork;
            fileService = _fileService;
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateLessonDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var section = await unitOfWork.Sections.GetByIdAsync(dto.SectionId);
            if (section == null)
                return NotFound("Section not found");

            var course = await unitOfWork.Courses.GetByIdAsync(section.CourseId);
            if (course.UserId != InstructorId)
                return Forbid();

            var url = await fileService.UploadFileAsync(dto.File, dto.Type);

            if (string.IsNullOrEmpty(url))
                return StatusCode(500, "An error occurred while uploading the file");

            Lesson newLesson = new()
            {
                SectionId = dto.SectionId,
                Title = dto.Title,
                Type = dto.Type,
                Order = dto.Order,
                FileUrl = url
            };

            if (dto.Type == FileType.Video)
                newLesson.DurationInSeconds = dto.DurationInSeconds;

            try
            {
                await unitOfWork.Lessons.AddAsync(newLesson);
                await unitOfWork.CompleteAsync();
                return Ok(new { lessonId = newLesson.Id });
            }
            catch
            {
                await fileService.DeleteFileAsync(url, dto.Type);
                return StatusCode(500, "An error occurred while creating the lesson");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await unitOfWork.Lessons.GetByIdAsync(id);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            var file = await fileService.DownloadFileAsync(lesson.FileUrl!, lesson.Type);

            if (file == null)
                return NotFound("File Not Found");

            if (lesson.Type == FileType.Video)
                Response.Headers.Append("Content-Disposition", $"inline; filename={file?.fileName}");

            return File(file.Value.stream, file.Value.contentType, file.Value.fileName);
        }


        [HttpGet("lessons-per-section/{sectionId}")]
        public async Task<IActionResult> GetLessonsPerSection(int sectionId)
        {
            var lessons = await unitOfWork.Lessons.GetLessonsBySectionId(sectionId);

            return Ok(lessons);
        }

    }
}