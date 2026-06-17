using ELProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ELProject.Shared.DTOs.Lessons
{
    public class CreateLessonDto
    {
        [Required]
        public IFormFile File { get; set; }
        public string Title { get; set; } = null!;
        public FileType Type { get; set; }
        public int? DurationInSeconds { get; set; }
    }
}
