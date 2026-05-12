namespace ELProject.Shared.DTOs
{
    public class UpdateSectionDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
    }
}
