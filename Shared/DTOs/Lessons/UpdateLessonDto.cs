using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Lessons
{
    public class UpdateLessonDto
    {
        public IFormFile? File { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public FileType Type { get; set; }
        public int? DurationInSeconds { get; set; }
    }
}
