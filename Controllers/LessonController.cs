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


        [HttpGet("course/{courseId}/lesson/{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId, int courseId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (studentId == null) return Unauthorized("User Not Authenticated");

            var IsEnrolledStudent = await unitOfWork.Enrollments.IsFoundAsync(studentId, courseId);

            if (!IsEnrolledStudent)
                return Forbid("You are not enrolled in this course");

            var lesson = await unitOfWork.Lessons.GetByIdAsync(lessonId);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            // 3. Generate short-lived SAS URL (no video bytes touch your server)
            var sasUrl = await fileService.GenerateSasUrlAsync(lesson.FileUrl!, lesson.Type, expiresInMinutes: 60);

            if (sasUrl == null)
            {
                var file = await fileService.DownloadFileAsync(lesson.FileUrl!, lesson.Type);

                if (file == null)
                    return NotFound("File Not Found");

                if (lesson.Type == FileType.Video)
                    Response.Headers.Append("Content-Disposition", $"inline; filename={file?.fileName}");

                return File(file.Value.stream, file.Value.contentType, file.Value.fileName);
            }

            // 4. For non-video files (PDF, Image) — just return the URL directly
            if (lesson.Type != FileType.Video)
                return Ok(new { fileUrl = sasUrl });

            // 5. For video — return URL + watermark payload
            var watermark = User.FindFirstValue(ClaimTypes.Email)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


            return Ok(new
            {
                lessonId = lesson.Id,
                title = lesson.Title,
                type = lesson.Type,
                order = lesson.Order,
                durationInSeconds = lesson.DurationInSeconds,
                fileUrl = sasUrl,
                watermark
            });
        }


        [HttpGet("lessons-per-section/{sectionId}")]
        public async Task<IActionResult> GetLessonsPerSection(int sectionId)
        {
            var lessons = await unitOfWork.Lessons.GetLessonsBySectionId(sectionId);

            return Ok(lessons);
        }


        [Authorize(Roles = "Instructor")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateLesson([FromForm] UpdateLessonDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var lesson = await unitOfWork.Lessons.GetLessonWithInstructorId(InstructorId, dto.Id);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(lesson.FileUrl))
                    await fileService.DeleteFileAsync(lesson.FileUrl, lesson.Type);

                var url = await fileService.UploadFileAsync(dto.File, dto.Type);
                lesson.FileUrl = url;
            }

            lesson.Title = dto.Title;
            lesson.Order = dto.Order;
            lesson.Type = dto.Type;

            if (dto.DurationInSeconds != null)
                lesson.DurationInSeconds = dto.DurationInSeconds;

            unitOfWork.Lessons.Update(lesson);
            await unitOfWork.CompleteAsync();
            return Ok("Lesson Updated Successfully");
        }


        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var lesson = await unitOfWork.Lessons.GetLessonWithInstructorId(InstructorId, id);//GetByIdAsync(id);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            if (!string.IsNullOrEmpty(lesson.FileUrl))
                await fileService.DeleteFileAsync(lesson.FileUrl, lesson.Type);

            unitOfWork.Lessons.Remove(lesson);
            await unitOfWork.CompleteAsync();
            return Ok("Lesson Deleted Successfully");
        }

    }
}