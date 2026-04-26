using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICloudStorageService cloudService;

        public LessonController(IUnitOfWork unitOfWork, ICloudStorageService _cloudService)
        {
            this.unitOfWork = unitOfWork;
            cloudService = _cloudService;
        }

        [HttpPost("upload-url/{fileName}/{type}")]
        public IActionResult GetUploadUrl(string fileName, FileType type)
        {
            var url = cloudService.GenerateUploadSas(fileName, type);

            return Ok(new
            {
                uploadUrl = url
            });
        }

        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateNewLessonDto dto)
        {
            var url = await cloudService.UploadFileAsync(dto.File, dto.Type);

            Lesson newLesson = new()
            {
                Title = dto.Title,
                Order = dto.Order,
                Type = dto.Type,
                FileUrl = url,
                DurationInSeconds = dto.DurationInSeconds
            };

            await unitOfWork.Lessons.AddAsync(newLesson);
            await unitOfWork.CompleteAsync();

            return Ok(new { url });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await unitOfWork.Lessons.GetByIdAsync(id);

            if (lesson == null)
                return NotFound("Lesson Not Found");

            var x = await cloudService.DownloadFileAsync(lesson.FileUrl!, lesson.Type);

            if (x == null)
                return NotFound("File Not Found");

            return File(x.Value.stream, x.Value.contentType, x.Value.fileName);
        }

    }
}
