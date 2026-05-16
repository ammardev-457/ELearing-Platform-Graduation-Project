namespace ELProject.Shared.DTOs.Sections
{
    public class CreateSectionDto
    {
        public string Title { get; set; } = null!;
        public int CourseId { get; set; }
        public int Order { get; set; }
    }
}
