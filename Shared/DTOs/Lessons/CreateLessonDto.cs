using ELProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ELProject.Shared.DTOs.Lessons
{
    public class CreateLessonDto
    {
        public int SectionId { get; set; }

        [Required]
        public IFormFile File { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public FileType Type { get; set; }
        public int? DurationInSeconds { get; set; }
    }
}
