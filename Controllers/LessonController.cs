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
        [HttpPost("{courseId}/section/{sectionId}/create")]
        public async Task<IActionResult> CreateLesson(int courseId, int sectionId, [FromForm] CreateLessonDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var course = await unitOfWork.Courses.GetByIdAsync(courseId);
            if (course.UserId != InstructorId)
                return Forbid();

            var url = await fileService.UploadFileAsync(dto.File, dto.Type);

            if (string.IsNullOrEmpty(url))
                return StatusCode(500, "An error occurred while uploading the file");

            int order = await unitOfWork.Lessons.GetOrderOfLastLessonInSection(sectionId) ?? 0;

            Lesson newLesson = new()
            {
                SectionId = sectionId,
                Title = dto.Title,
                Type = dto.Type,
                Order = order + 1,
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
        public async Task<IActionResult> GetLessonById(int courseId, int lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User Not Authenticated");

            var lesson = await unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null)
                return NotFound("Lesson Not Found");

            if (User.IsInRole("Student"))
            {
                var IsEnrolledStudent = await unitOfWork.Enrollments.IsFoundAsync(userId, courseId);

                if (lesson.Order > 2 && !IsEnrolledStudent)
                    return Forbid();
            }
            else if (User.IsInRole("Instructor"))
            {
                var course = await unitOfWork.Courses.GetByIdAsync(courseId);
                if (course.UserId != userId)
                    return Forbid();
            }


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
                return Ok(new 
                {
                    leesonId = lesson.Id,
                    title = lesson.Title,
                    type = lesson.Type,
                    order = lesson.Order,
                    fileUrl = sasUrl
                });

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

            return Ok(lessons.Select(l => new
            {
                id = l.Id,
                title = l.Title,
                type = l.Type,
                order = l.Order,
                fileUrl = l.FileUrl,
                durationInSeconds = l.DurationInSeconds,
                sectionId
            }));
        }


        [Authorize(Roles = "Instructor")]
        [HttpPut("update/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromForm] UpdateLessonDto dto)
        {
            var InstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (InstructorId == null) return Unauthorized("User Not Authenticated");

            var lesson = await unitOfWork.Lessons.GetLessonWithInstructorId(InstructorId, lessonId);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(lesson.FileUrl))
                    await fileService.DeleteFileAsync(lesson.FileUrl, lesson.Type);

                var url = await fileService.UploadFileAsync(dto.File, dto.Type);
                lesson.FileUrl = url;
                lesson.DurationInSeconds = dto.DurationInSeconds;
            }

            lesson.Title = dto.Title ?? lesson.Title;
            lesson.Order = dto.Order == 0 ? lesson.Order : dto.Order;
            lesson.Type = dto.Type;

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