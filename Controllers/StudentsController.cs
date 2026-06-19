using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELProject.Controllers
{
    [ApiController]
    [Authorize(Roles = "Student")]
    [Route("api/[controller]")]
    public class StudentsController(IUnitOfWork unitOfWork, IFileStorageService fileStorage) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IFileStorageService _fileStorage = fileStorage;

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var profile = await _unitOfWork.Users.GetStudentProfileAsync(studentId);
            return profile == null
                ? NotFound(new { message = "Student profile not found." })
                : Ok(profile);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var dashboard = await _unitOfWork.Users.GetStudentDashboardAsync(studentId);
            return dashboard == null
                ? NotFound(new { message = "Student dashboard not found." })
                : Ok(dashboard);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized(new { message = "Invalid token." });

            var myCourses = await _unitOfWork.Users.GetMyCoursesAsync(studentId);

            // Return empty list instead of 404 — student simply has no courses yet
            return Ok(myCourses);
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile(EditStudentProfileDto dto)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var studentProfile = await _unitOfWork.Users.UpdateStudentProfile(studentId, dto);

            if (studentProfile == null)
                return NotFound();

            if (dto.Image != null)
            {
                if (studentProfile.PathOfImage != null)
                    await fileStorage.DeleteFileAsync(studentProfile.PathOfImage, FileType.Image);

                var imagePath = await fileStorage.UploadFileAsync(dto.Image, FileType.Image);
                studentProfile.PathOfImage = imagePath;
            }

            _unitOfWork.Users.Update(studentProfile);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetProfile), null);
        }

    }
}