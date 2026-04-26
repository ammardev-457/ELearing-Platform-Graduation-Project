using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs
{
    public class CreateNewLessonDto
    {
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public FileType Type { get; set; }
        public int? DurationInSeconds { get; set; }
        public IFormFile File { get; set; }

    }
}
