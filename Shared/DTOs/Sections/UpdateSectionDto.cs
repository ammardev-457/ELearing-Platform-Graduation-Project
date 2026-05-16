namespace ELProject.Shared.DTOs.Sections
{
    public class UpdateSectionDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
    }
}
